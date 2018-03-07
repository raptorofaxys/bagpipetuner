/*

Arduino chromatic tuner.
2008-2017 Frederic Hamel

As a modified version of the Arduino core library, the LiquidCrystal class is licensed under the LGPL.
See attached license.

The rest is released under the Attribution-NonCommercial 3.0 Unported Creative Commons license:
http://creativecommons.org/licenses/by-nc/3.0/

See the project writeup:
http://deambulatorymatrix.blogspot.com/2010/11/digital-chromatic-guitar-tuner-2008.html.

Note that this code was more or less hacked together in a marathon along with the building of the actual
device; it should *definitely* not be considered an example of production-quality code.

For all pitch detection-related code, see the YIN paper:
de Cheveigne, Alain and Kawahara, Hideki.  "YIN, a fundamental frequency estimator for speech and music", Journal
of the Acoustical Society of America, Vol 111(4), pp. 1917-30, April 2002.
http://recherche.ircam.fr/equipes/pcm/cheveign/pss/2002_JASA_YIN.pdf

 */

#include <avr/pgmspace.h>
#include <EEPROM.h>

#define ASSERT(x) \
	if (!x) \
	{ \
		while (1) \
		{ \
			Blink(); \
		} \
	}

// Useful to print compile-time integer values, since instanciating a template with this incomplete type will
// cause the compiler to include the value of N in the error message.  (On avr-gcc, this seems to only work
// with free-standing variables, not members.)
template<int N> struct PrintInt;

// This is a very naive way of writing a static assertion facility, but avr-gcc is *very* far from compliant - it
// will allow all sorts of illegal constructs - and my patience is running out. :P  Even though this will generate
// a bit of code, it can hopefully be trivially stripped.
#define STATIC_ASSERT3(x, line) void func##line(static_assert_<x>) { }
#define STATIC_ASSERT2(x, line) STATIC_ASSERT3(x, line)
#define STATIC_ASSERT(x) STATIC_ASSERT2((x), __LINE__)
template <bool N> struct static_assert_;
template<> struct static_assert_<true> {};

// template <typename T> struct Identity { typedef T value; }; 

// template <typename T>
// T Clamp(T v, typename Identity<T>::value min_, typename Identity<T>::value max_)
// {
// 	return std::min(std::max(v, min_), max_);
// }

float Clamp(float v, float min_, float max_)
{
	return min(max(v, min_), max_);
}

// from http://stackoverflow.com/questions/1903954/is-there-a-standard-sign-function-signum-sgn-in-c-c
template <typename T> int sgn(T val)
{
    return (T(0) < val) - (val < T(0));
}

// from wiring_private.h
#ifndef cbi
#define cbi(sfr, bit) (_SFR_BYTE(sfr) &= ~_BV(bit))
#endif
#ifndef sbi
#define sbi(sfr, bit) (_SFR_BYTE(sfr) |= _BV(bit))
#endif 

// from pins_arduino.h
//extern const uint8_t PROGMEM port_to_input_PGM[];
extern const uint8_t PROGMEM digital_pin_to_port_PGM[];
extern const uint8_t PROGMEM digital_pin_to_bit_mask_PGM[];
extern const uint8_t PROGMEM digital_pin_to_timer_PGM[];
#define digitalPinToPort(P) ( pgm_read_byte( digital_pin_to_port_PGM + (P) ) )
#define digitalPinToBitMask(P) ( pgm_read_byte( digital_pin_to_bit_mask_PGM + (P) ) )
#define digitalPinToTimer(P) ( pgm_read_byte( digital_pin_to_timer_PGM + (P) ) )
//#define portInputRegister(P) ( (volatile uint8_t *)( pgm_read_byte( port_to_input_PGM + (P))) )

#define ENABLE_STARTUP_MESSAGE 0
#define ENABLE_PRINT 1
#define ENABLE_LCD 0
#define ENABLE_BUTTON_INPUTS 0
#define PRINT_FREQUENCY_TO_SERIAL 1
#define PRINT_FREQUENCY_TO_SERIAL_VT100 0
#define FAKE_FREQUENCY 0
#define ENABLE_DEBUG_PRINT_STATEMENTS 0
//#define Serial _SERIAL_FAIL_

#if ENABLE_DEBUG_PRINT_STATEMENTS
#define DEBUG_PRINT_STATEMENTS(x) x
#else
#define DEBUG_PRINT_STATEMENTS(x)
#endif

const char* COMMA_SEPARATOR = ", ";

#define EQUAL_TEMPERAMENT 1
#define JUST_TEMPERAMENT 2
#define TEMPERAMENT JUST_TEMPERAMENT

static int const kStatusPin = 13;
static int const kStatusPin2 = 12;
static int const kPitchDownButtonPin = 9;
static int const kPitchUpButtonPin = 10;
static int const kModeButtonPin = 11;

#include <inttypes.h>
#include "Print.h"

#if ENABLE_LCD
///////////////////////////////////////////////////////////////////////////////
// LCD code.  This began as a straight copy of the Arduino LiquidCrystal library,
// and it was tweaked afterwards for the MTC-S16205DFYHSAY.
// Timings *should* still work for the HD44780, but this particular model
// requires a bit of extra "convincing" on initialisation.
///////////////////////////////////////////////////////////////////////////////

static int const CHARACTER_WIDTH = 5;
static int const CHARACTER_HEIGHT = 7;
static int const DISPLAY_WIDTH = 16;
static int const MAX_TICK = CHARACTER_WIDTH * (DISPLAY_WIDTH - 1);
typedef char Glyph[CHARACTER_HEIGHT];
typedef int WideGlyph[CHARACTER_HEIGHT];

Glyph g_tickGlyph =
{
	0b00011111,
	0b00011111,
	0b00001110,
	0b00001110,
	0b00000100,
	0b00000100,
	0b00000100
};
static int const TICK_GLYPH = CHARACTER_WIDTH;
static int const NOTE_GLYPH = TICK_GLYPH + 1; // wide

class LiquidCrystal : public Print {
public:
  LiquidCrystal(uint8_t, uint8_t, uint8_t, uint8_t, uint8_t, uint8_t, uint8_t);
  LiquidCrystal(uint8_t, uint8_t, uint8_t, uint8_t, uint8_t, uint8_t, uint8_t,
    uint8_t, uint8_t, uint8_t, uint8_t);
  void clear();
  void home();
  void setCursor(int, int); 
  virtual size_t write(uint8_t);

  void setCharacterGlyph(uint8_t character, Glyph g);
  void setWideCharacterGlyph(uint8_t character, WideGlyph g);
private:
  void send(uint8_t, uint8_t);
  void command(uint8_t);
  
  uint8_t _four_bit_mode;
  uint8_t _rs_pin; // LOW: command.  HIGH: character.
  uint8_t _rw_pin; // LOW: write to LCD.  HIGH: read from LCD.
  uint8_t _enable_pin; // activated by a HIGH pulse.
  uint8_t _data_pins[8];
};

LiquidCrystal::LiquidCrystal(uint8_t rs, uint8_t rw, uint8_t enable,
  uint8_t d0, uint8_t d1, uint8_t d2, uint8_t d3) :
  _four_bit_mode(1), _rs_pin(rs), _rw_pin(rw), _enable_pin(enable)
{
  _data_pins[0] = d0;
  _data_pins[1] = d1;
  _data_pins[2] = d2;
  _data_pins[3] = d3; 
  
  pinMode(_rs_pin, OUTPUT);
  pinMode(_rw_pin, OUTPUT);
  pinMode(_enable_pin, OUTPUT);
  
  for (int i = 0; i < 4; i++)
    pinMode(_data_pins[i], OUTPUT);
 
  delay(250);
  command(0x28);  // function set: 4 bits, 1 line, 5x8 dots @fhamel: 2 lines for MTC-S16205DFYHSAY?
  delay(15);
  command(0x28);  // (again)
  delay(15);
  command(0x28);  // (again)
  delay(15);
  command(0x28);  // (again)
  delay(15);
  command(0x0C);  // display control: turn display on, cursor off, no blinking
  delay(15);
  command(0x0C);  // (again)
  delay(15);
  command(0x0C);  // (again)
  delay(15);
  command(0x0C);  // (again)
  delay(15);
  command(0x06);  // entry mode set: increment automatically, display shift, right shift
  delay(15);
  command(0x06);  // (again)
  delay(15);
  command(0x06);  // (again)
  delay(15);
  command(0x06);  // (again)
  delay(15);
  clear();
  delay(15);
}

void LiquidCrystal::clear()
{
  command(0x01);  // clear display, set cursor position to zero
  delayMicroseconds(2000);
}

void LiquidCrystal::home()
{
  command(0x02);  // set cursor position to zero
  delayMicroseconds(2000);
}

// Moves the cursor around on the LCD.
void LiquidCrystal::setCursor(int col, int row)
{
  int row_offsets[] = { 0x00, 0x40, 0x14, 0x54 };
  command(0x80 | (col + row_offsets[row]));
}

// Customizes a single character glyph in the LCD's CGRAM.
void LiquidCrystal::setCharacterGlyph(uint8_t characterIndex, Glyph g)
{
	// Index into the CGRAM, which is indexed with 3 MSB indicating glyph number and 3 LSB indicating row number.
	// However, there is no need to reset the address every time, since the entry mode's advance bit applies here
	// as well (i.e. the address autoincrements after each access).
	command(0x40 | (characterIndex << 3));
	for (int row = 0; row < CHARACTER_HEIGHT; ++row)
	{
		//command(0x40 | (characterIndex << 3) | row);
		write(g[row]);
	}
	write(0);
}

// Customizes a set pair of custom character glyps in the LCD's CGRAM; used to render large note names which are
// easier to read from afar.
void LiquidCrystal::setWideCharacterGlyph(uint8_t characterIndex, WideGlyph g)
{
	command(0x40 | (characterIndex << 3));
	for (int row = 0; row < CHARACTER_HEIGHT; ++row)
	{
		write(char((g[row] >> 5) & 0x1F));
	}
	write(0);
	command(0x40 | ((characterIndex + 1) << 3));
	for (int row = 0; row < CHARACTER_HEIGHT; ++row)
	{
		write(char(g[row] & 0x1F));
	}
	write(0);
}

void LiquidCrystal::command(uint8_t value) {
  send(value, LOW);
}

size_t LiquidCrystal::write(uint8_t value) {
  send(value, HIGH);
  return 1;
}

void LiquidCrystal::send(uint8_t value, uint8_t mode) {
  digitalWrite(_rs_pin, mode);
  digitalWrite(_rw_pin, LOW);

  if (_four_bit_mode) {
    for (int i = 0; i < 4; i++) {
      digitalWrite(_data_pins[i], (value >> (i + 4)) & 0x01);
    }
    
    digitalWrite(_enable_pin, HIGH);
    digitalWrite(_enable_pin, LOW);
    
    for (int i = 0; i < 4; i++) {
      digitalWrite(_data_pins[i], (value >> i) & 0x01);
    }

    digitalWrite(_enable_pin, HIGH);
    digitalWrite(_enable_pin, LOW);
  } else {
    for (int i = 0; i < 8; i++) {
      digitalWrite(_data_pins[i], (value >> i) & 0x01);
    }

    digitalWrite(_enable_pin, HIGH);
    digitalWrite(_enable_pin, LOW);
  }
}
LiquidCrystal* g_lcd;
#endif // #if ENABLE_LCD

///////////////////////////////////////////////////////////////////////////////
// Utility stuff
///////////////////////////////////////////////////////////////////////////////

const int INT_MIN = (1 << (sizeof(int) * 8 - 1));
const int INT_MAX = (~INT_MIN);  

// From http://www.arduino.cc/playground/Code/AvailableMemory
// This function will return the number of bytes currently free in RAM.
// written by David A. Mellis
// based on code by Rob Faludi http://www.faludi.com
// (reformatted for clarity)
// Note: I believe this does not work on recent versions of the Arduino environment.
// See the webpage above for alternatives.
int AvailableMemory()
{
	int size = 1024;
	byte* buf;

	while ((buf = (byte *) malloc(--size)) == NULL)
		;

	free(buf);

	return size;
}

#if ENABLE_PRINT

#if ENABLE_LCD
#define DEFAULT_PRINT g_lcd
#else
#define DEFAULT_PRINT (&Serial)
#endif

void Ln(Print* p = DEFAULT_PRINT)
{
	p->println("");
}

void ClearScreen(Print* p = DEFAULT_PRINT)
{
	p->print("\x1B[2J");
}

void MoveCursor(int x, int y, Print* p = DEFAULT_PRINT)
{
	p->print("\x1B[");
	p->print(y + 1);
	p->print(";");
	p->print(x + 1);
	p->print("H");
}

void Space(Print* p = DEFAULT_PRINT)
{
	p->print(" ");
}

void Cls(Print* p = DEFAULT_PRINT)
{
	p->print(char(27));
	p->print("[2J");
}

void PrintFloat(float f, int decimals = 10, Print* p = DEFAULT_PRINT)
{
	if (f < 0)
	{
		p->print("-");
		f = -f;
	}
	else
	{
		p->print(" ");
	}
	
	int b = int(f);
	p->print(b);
	p->print(".");
	f -= b;
	for (int i = 0; i < decimals; ++i)
	{
		f *= 10.0f;
		int a = int(f);
		p->print(a);
		f -= a;
	}
}

void PrintHex(int h, Print* p = DEFAULT_PRINT)
{
	static char const* hex = "0123456789ABCDEF";
	p->print(hex[(h & 0xF000) >> 12]);
	p->print(hex[(h & 0x0F00) >> 8]);
	p->print(hex[(h & 0x00F0) >> 4]);
	p->print(hex[(h & 0x000F) >> 0]);
}

void PrintStringInt(char const* s, int v, Print* p = DEFAULT_PRINT)
{
	p->print(s);
	p->print(": ");
	p->print(v);
}

void PrintStringLong(char const* s, long v, Print* p = DEFAULT_PRINT)
{
	p->print(s);
	p->print(": ");
	p->print(v);
}

void PrintStringFloat(char const* s, float f, int decimals = 5, Print* p = DEFAULT_PRINT)
{
	p->print(s);
	p->print(": ");
	if (f < 10000.0f)
	{
		p->print(" ");
	}
	if (f < 1000.0f)
	{
		p->print(" ");
	}
	if (f < 100.0f)
	{
		p->print(" ");
	}
	if (f < 10.0f)
	{
		p->print(" ");
	}
	PrintFloat(f, decimals);
}

#endif //ENABLE_PRINT

//@HACK
#include "tuner_utils.h"

//#define PROFILE_STATEMENT(count, msg, x) { unsigned long startMs = millis(); for (unsigned long i = 0; i < count; ++i) { x } unsigned long endMs = millis(); float timePerIteration = static_cast<float>(endMs - startMs) / count; PrintStringFloat(msg, timePerIteration); Ln(); }
//#define PROFILE_STATEMENT(count, msg, x) { unsigned long startUs = micros(); for (unsigned long i = 0; i < count; ++i) { x } unsigned long endUs = micros(); double timePerIteration = static_cast<double>(endUs - startUs) / count; PrintStringFloat(msg, static_cast<float>(timePerIteration)); Ln(); }

const int kDebounceMs = 50;

void Blink(int times = 1)
{
	for (int i = 0; i < times; ++i)
	{
		digitalWrite(kStatusPin, HIGH);
		delay(kDebounceMs);
		digitalWrite(kStatusPin, LOW);
		delay(kDebounceMs);
	}
}

#if ENABLE_BUTTON_INPUTS
class PushButton
{
public:
	PushButton(int pin, bool activeLow = true)
		: m_pin(pin)
		, m_debounceStart(0)
		, m_activeLow(activeLow)
	{
		pinMode(m_pin, INPUT);
	}
	
	void Update()
	{
		m_justPressed = false;
		m_justReleased = false;

		if (millis() < m_debounceStart + kDebounceMs)
		{
			return;
		}
		
		int buttonValue = digitalRead(m_pin);
		int const ACTIVE = m_activeLow ? 0 : 1;
		int const INACTIVE = 1 - ACTIVE;
		if ((m_lastValue == INACTIVE) && (buttonValue == ACTIVE))
		{
			m_debounceStart = millis();
			m_justPressed = true;
		}
		else if ((m_lastValue == ACTIVE) && (buttonValue == INACTIVE))
		{
			m_debounceStart = millis();
			m_justReleased = true;
		}
		m_lastValue = buttonValue;
	}
	
	bool IsPressed() { return m_lastValue == (m_activeLow ? 0 : 1); }
	bool JustPressed() { return m_justPressed; }
	bool JustReleased() { return m_justReleased; }

	void WaitForPress()
	{
		for (; !JustPressed(); Update());
		Update();
	}
	
private:
	int m_pin;
	int m_lastValue;
	unsigned long m_debounceStart;
	bool m_activeLow;
	bool m_justPressed;
	bool m_justReleased;
};
#else
class PushButton
{
public:
	PushButton(int, bool = true) {}
	void Update() {}
	bool IsPressed() { return false; }
	bool JustPressed() { return false; }
	bool JustReleased() { return false; }

	void WaitForPress() {}
};
#endif // #if ENABLE_BUTTON_INPUTS

PushButton g_pitchDownButton(kPitchDownButtonPin);
PushButton g_pitchUpButton(kPitchUpButtonPin);
PushButton g_modeButton(kModeButtonPin);

///////////////////////////////////////////////////////////////////////////////
// The Tuner
///////////////////////////////////////////////////////////////////////////////
// Note Freq    Cycles
// A	110		145454.55
// A#	116.54	137290.81
// B	123.47	129585.27
// C	130.81	122312.21
// C#	138.59	115447.35
// D	146.83	108967.79
// D#	155.56	102851.9
// E	164.81	97079.26
// F	174.61	91630.62
// F#	185		86487.79
// G	196		81633.6
// G#	207.65	77051.86
// A	220		72727.27

static int const NUM_NOTES = 13;
static const char* g_noteNames[NUM_NOTES] =
{
	"A ",
	"A#",
	"B ",
	"C ",
	"C#",
	"D ",
	"D#",
	"E ",
	"F ",
	"F#",
	"G ",
	"G#",
	"A "
};

int g_noteGlyphIndex[NUM_NOTES] =
{
	0,
	0,
	1,
	2,
	2,
	3,
	3,
	4,
	5,
	5,
	6,
	6,
	0
};

bool g_noteSharpSign[NUM_NOTES] =
{
	false,
	true,
	false,
	false,
	true,
	false,
	true,
	false,
	false,
	true,
	false,
	true,
	false
};

#if ENABLE_LCD
WideGlyph g_noteGlyphs[7] =
{
	{//   9876543210
		0b0000110000,
		0b0011111100,
		0b0110000110,
		0b0111111110,
		0b0111111110,
		0b0110000110,
		0b0110000110
	},
	{
		0b0111111000,
		0b0111111100,
		0b0110001100,
		0b0111111110,
		0b0110000110,
		0b0111111110,
		0b0111111110
	},
	{
		0b0011111100,
		0b0111111110,
		0b0110000110,
		0b0110000000,
		0b0110000110,
		0b0111111110,
		0b0011111100
	},
	{
		0b0111111000,
		0b0111111100,
		0b0110000110,
		0b0110000110,
		0b0110000110,
		0b0111111100,
		0b0111111000
	},
	{
		0b0111111110,
		0b0111111110,
		0b0110000000,
		0b0111111000,
		0b0110000000,
		0b0111111110,
		0b0111111110
	},
	{
		0b0111111110,
		0b0111111110,
		0b0110000000,
		0b0111111000,
		0b0110000000,
		0b0110000000,
		0b0110000000
	},
	{
		0b0011111100,
		0b0111111110,
		0b0110000000,
		0b0110001110,
		0b0110000110,
		0b0111111110,
		0b0011111110
	}
};
#endif // #if ENABLE_LCD

#if (TEMPERAMENT == EQUAL_TEMPERAMENT)
float const DEFAULT_A440 = 440.0f;
#elif (TEMPERAMENT == JUST_TEMPERAMENT)
float const DEFAULT_A440 = 479.0f;
#else
#error Unknown temperament!
#endif 

static int const A440_NOTE = 69;
static int const A880_NOTE = A440_NOTE + 12;

float const FREQUENCY_FILTER_WINDOW_RATIO_UP = 1.02f; 
float const FREQUENCY_FILTER_WINDOW_RATIO_DOWN = 1.0f / FREQUENCY_FILTER_WINDOW_RATIO_UP; 

float const QUARTERTONE_UP = 1.0293022366f;
float const QUARTERTONE_DOWN = 0.9715319412f;
#if (TEMPERAMENT == EQUAL_TEMPERAMENT)
float const SEMITONE_UP = 1.0594630944f;
float const SEMITONE_DOWN = 0.9438743127f;
#elif (TEMPERAMENT == JUST_TEMPERAMENT)
float g_noteSemitoneRatio[] =
{
	1.0f,
	1.0f + 1 / 15.0f,
	1.0f + 1 / 8.0f,
	1.0f + 1 / 5.0f,
	1.0f + 1 / 4.0f,
	1.0f + 1 / 3.0f,
	//1.0f + 7 / 20.0f,
	1.0f + 13.0f / 32.0f,
	1.0f + 1 / 2.0f,
	1.0f + 3.0f / 5.0f,
	1.0f + 2.0f / 3.0f,
	1.0f + 3.0f / 4.0f,
	//1.0f + 7.0 / 9.0f, 
	//1.0f + 4.0 / 5.0f,
	1.0f + 7.0 / 8.0f,
	2.0f,
};
#else
#error Unknown temperament!
#endif

typedef int Fixed;
static int const FIXED_SHIFT = 5; // @TODO: can we bump this up to 6? where is the issue? (2*6=12 => 4 bits left for integer portion)
static int const FIXED_ONE = (1 << FIXED_SHIFT);
static int const FRAC_MASK = (1 << FIXED_SHIFT) - 1;
//static int const FIXED_SHIFT_HI = 8 - FIXED_SHIFT;
//static int const FIXED_MASK_HI = ~((1 << FIXED_SHIFT_HI) - 1);
#define FIXED_INT(x) int(x >> FIXED_SHIFT)
#define FIXED_FRAC(x) int(x & FRAC_MASK)
#define		I2FIXED(x) ((Fixed) ((x) << FIXED_SHIFT))
#define		F2FIXED(x) ((Fixed) ((x) * (1 << FIXED_SHIFT)))
#define		FIXED2I(x) ((x) >> FIXED_SHIFT)
#define		FIXED2F(x) ((x) / float(1 << FIXED_SHIFT))
static int const PRIME_SHIFT = 8;

namespace TunerMode
{
	enum Type
	{
		Tuner = 0,
		Max
	};
}

static int const PRESCALER = 0b00000101;
static int const PRESCALER_DIVIDE = (1 << PRESCALER);
static int const ADC_CLOCKS_PER_ADC_CONVERSION = 13;
static unsigned long const CPU_CYCLES_PER_SAMPLE = ADC_CLOCKS_PER_ADC_CONVERSION * PRESCALER_DIVIDE;
static unsigned long const SAMPLES_PER_SECOND = F_CPU / CPU_CYCLES_PER_SAMPLE;

static int const ABSOLUTE_MIN_FREQUENCY = 76;
static int const ABSOLUTE_MAX_FREQUENCY = 1100;
static int const ABSOLUTE_MIN_SAMPLES = SAMPLES_PER_SECOND / ABSOLUTE_MAX_FREQUENCY;
static int const ABSOLUTE_MAX_SAMPLES = SAMPLES_PER_SECOND / ABSOLUTE_MIN_FREQUENCY;
//static int const WINDOW_SIZE = ABSOLUTE_MAX_SAMPLES; //96; // samples
static int const MAX_BUFFER_SIZE = 2 * ABSOLUTE_MAX_SAMPLES + 1; // for interpolation
//STATIC_ASSERT(true);
STATIC_ASSERT((SAMPLES_PER_SECOND / 2) > ABSOLUTE_MAX_FREQUENCY);
//STATIC_ASSERT(ABSOLUTE_MAX_SAMPLES <= 255); // so we can use unsigned chars as offsets; this is not the case with a prescaler of 101b and a minimum frequency of 75 Hz

template <unsigned long i> struct PrintN;
//PrintN<SAMPLES_PER_SECOND> n1;
//PrintN<BUFFER_SIZE> n1;
//PrintN<WINDOW_SIZE> n1;
//PrintN<MIN_SAMPLES> n1;
//PrintN<SAMPLES_PER_SECOND> n;
//PrintN<ABSOLUTE_MAX_SAMPLES> n;
//PrintN<ABSOLUTE_MIN_SAMPLES> n;

// This buffer is now global because we need to the compiler to use faster addressing modes that are only available with
// fixed memory addresses. The buffer is now shared between the different tuner channels.
char g_recordingBuffer[MAX_BUFFER_SIZE];

bool g_dumpOnNullReading = false;
float g_dumpBelowFrequency = -1.0f;

namespace DumpMode
{
    enum Type
    {
        DumpBuffer,
        DumpGcf
    };
}

DumpMode::Type g_dumpMode = DumpMode::DumpGcf;

class Tuner
{
public:
	static int const NUM_CHANNELS = 4;

	class Channel
	{
	public:
		Channel()
		{
			m_enableDetailedSearch = true;

			m_gcfStep = 4;
			m_correlationDipThresholdPercent = 25;
			m_baseOffsetStep = 4;
			m_baseOffsetStepIncrement = 2;

			m_minFrequency = ABSOLUTE_MIN_FREQUENCY;
			m_maxFrequency = ABSOLUTE_MAX_FREQUENCY;
		}

		void SetPin(int audioPin)
		{
			m_audioPin = audioPin;
		}

		void SelectADCChannel()
		{
			// Select input channel + set reference to Vcc
			ADMUX = /*(0 << 6) |*/ (m_audioPin & 0x0f);
			ADMUX = (1 << 6) | (m_audioPin & 0x0f);

			// Disable other ADC channels (try to reduce noise?)
			DIDR0 = (0x3F ^ (1 << m_audioPin));
		}

		// This function requires proper setup in Tuner::Start() and SelectADCChannel()
		unsigned int ReadInput8BitsUnsigned()
		{
			while ((ADCSRA & _BV(ADIF)) == 0)
			{
			}

			unsigned int result = ADCH;
			sbi(ADCSRA, ADIF);
			//PrintStringInt("ReadInput8BitsUnsigned", result); Ln();
			return result;
		}

		int ReadInput8BitsSigned()
		{
			int result = ReadInput8BitsUnsigned() - 128;
			//PrintStringInt("ReadInput8BitsSigned", result); Ln();
			return result;
		}

		// Linearly interpolates between two 8-bit signed values.
		char InterpolateChar_old(char a, char b, char tFrac)
		{
			int d = b - a;
			//@TODO: test casting to char before the addition to save on the 16-bit add
			return a + ((d * tFrac) >> FIXED_SHIFT);
		}

		// Linearly interpolates between two 8-bit signed values.
		char InterpolateChar(char a, char b, char tFrac)
		{
			int d = b - a;
			return a + static_cast<char>((d * tFrac) >> FIXED_SHIFT);
		}

		//void __attribute__ ((noinline)) foo() 
		//{
		//x
		//}

		//attribute ((noinline)) void foo() { x }

		//unsigned long __attribute__((noinline)) GetCorrelationFactorFixed_old(Fixed fixedOffset, int windowSize, int correlationStep)
		//{
		//	unsigned long result = 0;
		//	int integer = FIXED_INT(fixedOffset);
		//	int frac = FIXED_FRAC(fixedOffset);

		//	// If we're in MIDI mode, lower the precision to gain speed.
		//	//if (m_mode == TunerMode::Midi)
		//	//{
		//	//	correlationStep <<= 1;
		//	//}

		//	char* pA = &g_recordingBuffer[0];
		//	char* pB = &g_recordingBuffer[integer];
		//	char* pB2 = pB + 1;

		//	//@OPTIMIZE: for B, move b2 into b, then indirect-read the new b2? might not make a difference
		//	// No: we are skipping ahead by correlation step, which is usually well above 1, so we need to read both values anyway
		//	for (int i = 0; i < windowSize; i += correlationStep, pA += correlationStep, pB += correlationStep, pB2 += correlationStep)
		//	{
		//		// Note this is done with 16-bit math; this is slower, but gives more precision. In tests, using 8-bit fixed-point
		//		// math did not yield sufficient precision.
		//		int a = *pA;
		//		int b = InterpolateChar_old(*pB, *pB2, frac);
		//		result += abs(b - a);
		//	}
		//	return result;
		//}

		//unsigned long __attribute__((noinline)) GetCorrelationFactorFixed(Fixed fixedOffset, int windowSize, int correlationStep)
		//{
		//	unsigned long result = 0;
		//	int integer = FIXED_INT(fixedOffset);
		//	int frac = FIXED_FRAC(fixedOffset);

		//	// If we're in MIDI mode, lower the precision to gain speed.
		//	//if (m_mode == TunerMode::Midi)
		//	//{
		//	//	correlationStep <<= 1;
		//	//}

		//	char* pA = &g_recordingBuffer[0];
		//	char* pB = &g_recordingBuffer[integer];
		//	char* pB2 = pB + 1;
		//	char* pEndA = pA + windowSize;

		//	for (; pA < pEndA; pA += correlationStep, pB += correlationStep, pB2 += correlationStep)
		//	{
		//		// Note this is done with 16-bit math; this is slower, but gives more precision. In tests, using 8-bit fixed-point
		//		// math did not yield sufficient precision.
		//		int a = *pA;
		//		//@TODO: test doing this math on a char; does this require shifting all samples right by 1?
		//		int b = InterpolateChar_old(*pB, *pB2, frac);
		//		result += abs(b - a);
		//	}
		//	return result;
		//}

		unsigned long __attribute__((noinline)) GetCorrelationFactorFixed(Fixed fixedOffset, int windowSize, int correlationStep)
		{
			unsigned long result = 0;
			int integer = FIXED_INT(fixedOffset);
			int frac = FIXED_FRAC(fixedOffset);

			// If we're in MIDI mode, lower the precision to gain speed.
			//if (m_mode == TunerMode::Midi)
			//{
			//	correlationStep <<= 1;
			//}

			char* pA = &g_recordingBuffer[0];
			char* pB = &g_recordingBuffer[integer];
			char* pB2 = pB + 1;
			char* pEndA = pA + windowSize;

			for (; pA < pEndA; pA += correlationStep, pB += correlationStep, pB2 += correlationStep)
			{
				// Note this is done with 16-bit math; this is slower, but gives more precision. In tests, using 8-bit fixed-point
				// math did not yield sufficient precision.
				int a = *pA;
				//@TODO: test doing this math on a char; does this require shifting all samples right by 1?
				//Answer: not any faster
				int b = InterpolateChar(*pB, *pB2, frac);
				result += abs(b - a);
			}
			return result;
		}

		//unsigned long __attribute__((noinline)) GetCorrelationFactorFixed3(Fixed fixedOffset, int windowSize, int correlationStep)
		//{
		//	unsigned long result = 0;
		//	int integer = FIXED_INT(fixedOffset);
		//	int frac = FIXED_FRAC(fixedOffset);

		//	// If we're in MIDI mode, lower the precision to gain speed.
		//	//if (m_mode == TunerMode::Midi)
		//	//{
		//	//	correlationStep <<= 1;
		//	//}

		//	char* pA = &g_recordingBuffer[0];
		//	char* pB = &g_recordingBuffer[integer];
		//	char* pB2 = pB + 1;

		//	//@OPTIMIZE: for B, move b2 into b, then indirect-read the new b2? might not make a difference
		//	// No: we are skipping ahead by correlation step, which is usually well above 1, so we need to read both values anyway
		//	for (int i = 0; i < windowSize; i += correlationStep, pA += correlationStep, pB += correlationStep, pB2 += correlationStep)
		//	{
		//		// Note this is done with 16-bit math; this is slower, but gives more precision. In tests, using 8-bit fixed-point
		//		// math did not yield sufficient precision.
		//		int a = *pA;
		//		int b = InterpolateChar(*pB, *pB2, frac);
		//		result += abs(b - a);
		//	}
		//	return result;
		//}

		//unsigned long __attribute__((noinline)) GetCorrelationFactorFixed4(Fixed fixedOffset, int windowSize, int correlationStep)
		//{
		//	unsigned long result = 0;
		//	int integer = FIXED_INT(fixedOffset);
		//	int frac = FIXED_FRAC(fixedOffset);

		//	// If we're in MIDI mode, lower the precision to gain speed.
		//	//if (m_mode == TunerMode::Midi)
		//	//{
		//	//	correlationStep <<= 1;
		//	//}

		//	char* pA = &g_recordingBuffer[0];
		//	char* pB = &g_recordingBuffer[integer];
		//	char* pB2 = pB + 1;

		//	//@OPTIMIZE: for B, move b2 into b, then indirect-read the new b2? might not make a difference
		//	// No: we are skipping ahead by correlation step, which is usually well above 1, so we need to read both values anyway
		//	for (int i = 0; i < windowSize; i += correlationStep, pA += correlationStep, pB += correlationStep, pB2 += correlationStep)
		//	{
		//		// Note this is done with 16-bit math; this is slower, but gives more precision. In tests, using 8-bit fixed-point
		//		// math did not yield sufficient precision.
		//		char a = *pA;
		//		char b = InterpolateChar(*pB, *pB2, frac);
		//		result += abs(b - a);
		//	}
		//	return result;
		//}

		//unsigned long GetCorrelationFactorPrime(unsigned long currentCorrellation, int numToDate, unsigned long sumToDate)
		//{
		//	//if (numToDate == 0)
		//	//{
		//	//	return FIXED_ONE;
		//	//}
		//	//else
		//	//{
		//		return ((currentCorrellation << FIXED_SHIFT) * numToDate) / sumToDate;
		//	//}
		//}

		//unsigned long GetCorrelationFactorPrime2(unsigned long currentCorrelation, int numToDate, unsigned long sumToDate)
		//{
		//	// Here we don't care about the absolute value; we're just comparing values at various points on the curve to find a local minimum. The default fixed shift
		//	// isn't quite large enough to produce values which will be meaningful when divided by the potentially large sumToDate, so we increase the just for this
		//	// function.
		//	//if (numToDate == 0)
		//	//{
		//	//	//return (1 << PRIME_SHIFT);
		//	//	return 1;
		//	//}
		//	//else
		//	//{
		//		//return ((currentCorrelation << PRIME_SHIFT) * numToDate) / (sumToDate >> 8);
		//		return (currentCorrelation * numToDate) / (sumToDate >> 8);
		//	//}
		//}

		// Compute the frequency corresponding to a given a fixed-point offset into our sampling buffer (usually where
		// the best/minimal autocorrelation was achieved).
		float GetFrequencyForOffsetFixed(Fixed offset)
		{
			float floatOffset = FIXED2F(offset);
			return F_CPU / (floatOffset * CPU_CYCLES_PER_SAMPLE);
		}

		float DetermineSignalPitch(float& minFrequency, float& maxFrequency, int& signalMin, int& signalMax)
		{
			int minSamples = SAMPLES_PER_SECOND / m_maxFrequency;
			int maxSamples = SAMPLES_PER_SECOND / m_minFrequency;
			int bufferSize = 2 * maxSamples + 1;

			minFrequency = -1.0f;
			maxFrequency = -1.0f;

			//DEFAULT_PRINT->print("DetermineSignalPitch()"); Ln();
			static int const AMPLITUDE_THRESHOLD = 30;
			
			DEBUG_PRINT_STATEMENTS(Serial.write("DetermineSignalPitch()"); Ln(););

			// Sample the signal into our buffer, and track its amplitude.
			signalMin = INT_MAX;
			signalMax = INT_MIN;
			int maxAmplitude = -1;
			//for (int i = 0; i < BUFFER_SIZE; ++i)
			for (int i = 0; i < bufferSize; ++i)
			{
				g_recordingBuffer[i] = ReadInput8BitsSigned();
				signalMin = min(g_recordingBuffer[i], signalMin);
				signalMax = max(g_recordingBuffer[i], signalMax);
				maxAmplitude = max(maxAmplitude, abs(signalMax - signalMin));
			}

			//DEFAULT_PRINT->print("DetermineSignalPitch() done sampling"); Ln();
			//PrintStringInt("signalMin", signalMin); Ln();
			//PrintStringInt("signalMax", signalMax); Ln();
			//PrintStringInt("maxAmplitude", maxAmplitude); Ln();
			//for (int i = 0; i < BUFFER_SIZE; ++i)
			//{
			//	DEFAULT_PRINT->print(static_cast<int>(g_recordingBuffer[i]));
			//	if (i != BUFFER_SIZE - 1)
			//	{
			//		DEFAULT_PRINT->print(",");
			//	}
			//}
			//Ln();

			DEBUG_PRINT_STATEMENTS(
			{
				PrintStringInt("cdp", m_correlationDipThresholdPercent); Ln();
				PrintStringInt("gcfs", m_gcfStep); Ln();
				PrintStringInt("bos", m_baseOffsetStep); Ln();
				PrintStringInt("bosi", m_baseOffsetStepIncrement); Ln();
				PrintStringInt("minf", m_minFrequency); Ln();
				PrintStringInt("maxf", m_maxFrequency); Ln();
				PrintStringInt("bufferSize", bufferSize); Ln();
				PrintStringInt("signalMin", signalMin); Ln();
				PrintStringInt("signalMax", signalMax); Ln();
				PrintStringInt("maxAmplitude", maxAmplitude); Ln();
			});

			// If we haven't reached the amplitude threshold, don't try to determine pitch.
			if (maxAmplitude < AMPLITUDE_THRESHOLD)
			{
				//DEFAULT_PRINT->print("DetermineSignalPitch() no amplitude"); Ln();
				return -1.0f;
			}

			bool doPrint = false;
			Fixed minBestOffset = ~0;
			Fixed maxBestOffset = ~0;
			Fixed bestOffset = ~0;
            // Normally we only run once through the below code. This loop is only here in case we need to print the result of some computations. This is typically done when an abnormal result is obtained.
			for (;;)
			{
				// Alright, now try to figure what the ballpark note this is by calculating autocorrelation
				bool inThreshold = false;

				const Fixed maxSamplesFixed = I2FIXED(maxSamples);
				unsigned long maxCorrelation = 0;
				unsigned long correlationDipThreshold = 0;
				unsigned long bestCorrelation = ~0;
				const Fixed offsetToStartPreciseSampling = max(I2FIXED(minSamples - 2), 0);
				unsigned long lastCorrelation = ~0;

				Fixed offsetStep = m_baseOffsetStep;

                if (doPrint && (g_dumpMode == DumpMode::DumpBuffer))
                {
                    for (int i = 0; i < bufferSize; ++i)
                    {
                        DEFAULT_PRINT->print("#");
                        DEFAULT_PRINT->print(static_cast<int>(g_recordingBuffer[i]));
                        Ln();
                        //if (i != BUFFER_SIZE - 1)
                        //{
                        //	DEFAULT_PRINT->print(",");
                        //}
                    }
                    //Ln();
                }

				{
					const unsigned long iterations = 2000;

					// Look at assembler output?
					//DEFAULT_PRINT->print("#");
					//PROFILE_STATEMENT_LINE({ GetCorrelationFactorFixed_old(m_gcfStep, maxSamples, m_gcfStep); });
					//DEFAULT_PRINT->print("#");
					//PROFILE_STATEMENT_LINE({ GetCorrelationFactorFixed(m_gcfStep, maxSamples, m_gcfStep); });
					//DEFAULT_PRINT->print("#");
					//PROFILE_STATEMENT_LINE({ GetCorrelationFactorFixed2(m_gcfStep, maxSamples, m_gcfStep); });
					//DEFAULT_PRINT->print("#");
					//PROFILE_STATEMENT_LINE({ GetCorrelationFactorFixed3(m_gcfStep, maxSamples, m_gcfStep); });
					//DEFAULT_PRINT->print("#");
					//PROFILE_STATEMENT_LINE({ GetCorrelationFactorFixed4(m_gcfStep, maxSamples, m_gcfStep); });
					//DEFAULT_PRINT->print("#");
					//PROFILE_STATEMENT_LINE({ useless += GetCorrelationFactorFixed_old(m_gcfStep, maxSamples, m_gcfStep); });
					//DEFAULT_PRINT->print("#");
					//PROFILE_STATEMENT_LINE({ useless += GetCorrelationFactorFixed(m_gcfStep, maxSamples, m_gcfStep); });
					//DEFAULT_PRINT->print("#");
					//PROFILE_STATEMENT_LINE({ useless += GetCorrelationFactorFixed2(m_gcfStep, maxSamples, m_gcfStep); });
					//DEFAULT_PRINT->print("#");
					//PROFILE_STATEMENT_LINE({ useless += GetCorrelationFactorFixed3(m_gcfStep, maxSamples, m_gcfStep); });
					//DEFAULT_PRINT->print("#");
					//PROFILE_STATEMENT_LINE({ useless += GetCorrelationFactorFixed4(m_gcfStep, maxSamples, m_gcfStep); });
					//DEFAULT_PRINT->print("#");
					//PrintStringLong("useless", useless); Ln();
					//PROFILE_STATEMENT_LINE(iterations, "#GCFo", { volatile unsigned long gcf = GetCorrelationFactorFixed_old(0, maxSamples, m_gcfStep); });
					//PROFILE_STATEMENT_LINE(iterations, "#GCF", { volatile unsigned long gcf = GetCorrelationFactorFixed(0, maxSamples, m_gcfStep); });
					//PROFILE_STATEMENT_LINE(iterations, "#GCF2", { volatile unsigned long gcf = GetCorrelationFactorFixed2(0, maxSamples, m_gcfStep); });
					//PROFILE_STATEMENT_LINE(iterations, "#GCF3", { volatile unsigned long gcf = GetCorrelationFactorFixed3(0, maxSamples, m_gcfStep); });
					//PROFILE_STATEMENT(iterations, "#GCFo", { GetCorrelationFactorFixed_old(0, maxSamples, m_gcfStep); });
					//PROFILE_STATEMENT(iterations, "#GCF" , { GetCorrelationFactorFixed(0, maxSamples, m_gcfStep); });
					//PROFILE_STATEMENT(iterations, "#GCF2", { GetCorrelationFactorFixed2(0, maxSamples, m_gcfStep); });
				}

				//char t;

				//PROFILE_STATEMENT(300000UL, "#IC" , { t = InterpolateChar(5, 10, 10); });
				//DEFAULT_PRINT->print("#"); DEFAULT_PRINT->print(t); Ln();
				//PROFILE_STATEMENT(300000UL, "#IC2", { t = InterpolateChar(5, 10, 10); });
				//DEFAULT_PRINT->print("#"); DEFAULT_PRINT->print(t); Ln();

				// We start a bit before the minimum offset to prime the thresholds
				//for (Fixed offset = max(offsetAtMinFrequency - OFFSET_STEP * 4, 0); offset < maxSamplesFixed; offset += OFFSET_STEP)
				// make a function out of the subdivision loop; scan from offset to offset with a given step and adaptive parameters, with a given skip for GCF
				for (Fixed offset = (offsetToStartPreciseSampling >> 1); offset < maxSamplesFixed; )
				{
                    //@TODO: why the shift here? is this a legacy artifact?
                    //unsigned long curCorrelation = GetCorrelationFactorFixed(offset, 2) << 8; // was using 96, which worked for the simple function generator but didn't work quite as well for the bagpipe signal
					unsigned long curCorrelation = GetCorrelationFactorFixed(offset, maxSamples, m_gcfStep);
					//unsigned long curCorrelation2 = GetCorrelationFactorFixed4(offset, maxSamples, m_gcfStep);
					//if (curCorrelation != curCorrelation2)
					//{
					//	DEFAULT_PRINT->print("#");
					//	DEFAULT_PRINT->print("GCF discrepancy");
					//	Ln();
					//}

					if (doPrint && (g_dumpMode == DumpMode::DumpGcf))
					{
						//PrintStringInt("ofs", offset); DEFAULT_PRINT->print(" ");
						//PrintStringLong("gcf", curCorrelation); DEFAULT_PRINT->print(" "); Ln();
						DEFAULT_PRINT->print("#");
						DEFAULT_PRINT->print(offset); DEFAULT_PRINT->print(COMMA_SEPARATOR);
						DEFAULT_PRINT->print(curCorrelation); DEFAULT_PRINT->print(COMMA_SEPARATOR);
						//DEFAULT_PRINT->print(GetCorrelationFactorFixed(offset, 4) * 4); DEFAULT_PRINT->print(COMMA_SEPARATOR);
						//DEFAULT_PRINT->print(GetCorrelationFactorFixed(offset, 8) * 8); DEFAULT_PRINT->print(COMMA_SEPARATOR);
						//DEFAULT_PRINT->print(GetCorrelationFactorFixed(offset, 16) * 16); DEFAULT_PRINT->print(COMMA_SEPARATOR);
						//DEFAULT_PRINT->print(GetCorrelationFactorFixed(offset, 24) * 24); DEFAULT_PRINT->print(COMMA_SEPARATOR);
						//DEFAULT_PRINT->print(GetCorrelationFactorFixed(offset, 32) * 32); DEFAULT_PRINT->print(COMMA_SEPARATOR);
						//DEFAULT_PRINT->print(GetCorrelationFactorFixed(offset, 64) * 64); DEFAULT_PRINT->print(COMMA_SEPARATOR);
						//DEFAULT_PRINT->print(GetCorrelationFactorFixed(offset, 96) * 96); DEFAULT_PRINT->print(COMMA_SEPARATOR);
						DEFAULT_PRINT->print(maxCorrelation); DEFAULT_PRINT->print(COMMA_SEPARATOR);
						DEFAULT_PRINT->print(correlationDipThreshold);
						Ln();
					}

					if (curCorrelation > maxCorrelation)
					{
						maxCorrelation = curCorrelation;
						correlationDipThreshold = (maxCorrelation * m_correlationDipThresholdPercent) / 100;
						//PrintStringLong("maxCorrelation", maxCorrelation); DEFAULT_PRINT->print(" ");
						//PrintStringLong("correlationDipThreshold", correlationDipThreshold); Ln();
					}

					if (offset < offsetToStartPreciseSampling)
					{
						offset += m_baseOffsetStep * 4;
						continue;
					}

					if (curCorrelation < correlationDipThreshold)
					{
						if (curCorrelation < bestCorrelation)
						{
							bestCorrelation = curCorrelation;
							minBestOffset = offset;
							maxBestOffset = offset;
							//DEFAULT_PRINT->print("best!"); Ln();
						}
						else if (curCorrelation == bestCorrelation)
						{
							maxBestOffset = offset;
						}

						inThreshold = true;
						//DEFAULT_PRINT->print("enter threshold"); Ln();
					}
					else if (inThreshold) // was in threshold, now exited, have best minimum in threshold
					{
						//DEFAULT_PRINT->print("exit threshold"); Ln();
						break;
					}

					if (curCorrelation >= lastCorrelation)
					{
						offsetStep += m_baseOffsetStepIncrement;
					}
					else
					{
						offsetStep = m_baseOffsetStep;
					}

					offset += offsetStep;
					lastCorrelation = curCorrelation;
				}

				//if (doPrint)
				//{
				//	PrintStringInt("minBestOffset", minBestOffset); Ln();
				//	PrintStringInt("maxBestOffset", maxBestOffset); Ln();
				//	PrintStringInt("maxSamplesFixed", maxSamplesFixed); Ln();
				//}

                if (!doPrint
                    && (
                        ((g_dumpBelowFrequency >= 0.0f) && (minBestOffset != ~0) && (GetFrequencyForOffsetFixed(minBestOffset) < g_dumpBelowFrequency))
                        || (g_dumpOnNullReading && (minBestOffset == ~0))
                        )
                    )
                {
                    doPrint = true;
                    continue;
                }
                
                if (minBestOffset == ~0)
				{
					//DEFAULT_PRINT->print("bestOffset was never set"); Ln();
					return -1.0f;
				}

				// Refine the search for the offset that produces the true minimum autocorrelation
				// At this point, were we to use a very small GCF step, I think we could use a binary search
				// to find the minimum; it feels like we can rely on the area of the autocorrelation function
				// that we are exploring to be parabola-like with one actual minimum. However, a few
				// questions arise:
				// -is it faster to search fewer points with a small GCF step or more points with a bigger GCF step? (that depends)
				// -with a bigger GCF step, are we introducing sampling artifacts in the autocorrelation function which are big enough
				//  to through off the hunt for a minimum?
				//@TODO: try searching for the minimum with various GCF steps and output the found minimum at the various steps;
				// see how big we can make the step without affecting the accuracy of results

				if (!m_enableDetailedSearch)
				{
					break;
				}

				//@TODO: upsample in stages, reducing the GCF step gradually
				Fixed lowOffset = max(minBestOffset - offsetStep, 0);
				unsigned long lowGcf = GetCorrelationFactorFixed(lowOffset, maxSamples, 2);
				Fixed highOffset = maxBestOffset + offsetStep;
				unsigned long highGcf = GetCorrelationFactorFixed(highOffset, maxSamples, 2);

				DEBUG_PRINT_STATEMENTS(
				{
					//PrintStringInt("lowOffset", lowOffset); Ln();
					//PrintStringInt("lowGcf", lowGcf); Ln();
					//PrintStringInt("highOffset", highOffset); Ln();
					//PrintStringInt("highGcf", highGcf); Ln();
					PrintStringInt("minBestOffset", minBestOffset); Ln();
					PrintStringInt("maxBestOffset", maxBestOffset); Ln();
				});

				int iterationCount = 0;
				for (; iterationCount < 100; ++iterationCount)
				{
					Fixed lowOffsetOption = (2 * lowOffset + highOffset) / 3;
					unsigned long lowGcfOption = GetCorrelationFactorFixed(lowOffsetOption, maxSamples, 2);
					Fixed highOffsetOption = (lowOffset + 2 * highOffset) / 3;
					unsigned long highGcfOption = GetCorrelationFactorFixed(highOffsetOption, maxSamples, 2);
					
					//DEFAULT_PRINT->print("#");
					//DEFAULT_PRINT->print(m_minFrequency); DEFAULT_PRINT->print(COMMA_SEPARATOR);
					//DEFAULT_PRINT->print(m_maxFrequency); DEFAULT_PRINT->print(COMMA_SEPARATOR);
					//DEFAULT_PRINT->print(m_correlationDipThresholdPercent); DEFAULT_PRINT->print(COMMA_SEPARATOR);
					//DEFAULT_PRINT->print(m_gcfStep); DEFAULT_PRINT->print(COMMA_SEPARATOR);
					//DEFAULT_PRINT->print(m_baseOffsetStep); DEFAULT_PRINT->print(COMMA_SEPARATOR);
					//DEFAULT_PRINT->print(m_baseOffsetStepIncrement);
					//Ln();

					//DEFAULT_PRINT->print("#");
					//DEFAULT_PRINT->print(lowOffset); DEFAULT_PRINT->print(COMMA_SEPARATOR);
					//DEFAULT_PRINT->print(lowGcf); DEFAULT_PRINT->print(COMMA_SEPARATOR);
					//DEFAULT_PRINT->print(highOffset); DEFAULT_PRINT->print(COMMA_SEPARATOR);
					//DEFAULT_PRINT->print(highGcf); DEFAULT_PRINT->print(COMMA_SEPARATOR);
					//DEFAULT_PRINT->print(lowOffsetOption); DEFAULT_PRINT->print(COMMA_SEPARATOR);
					//DEFAULT_PRINT->print(lowGcfOption); DEFAULT_PRINT->print(COMMA_SEPARATOR);
					//DEFAULT_PRINT->print(highOffsetOption); DEFAULT_PRINT->print(COMMA_SEPARATOR);
					//DEFAULT_PRINT->print(highGcfOption); // DEFAULT_PRINT->print(COMMA_SEPARATOR);
					//Ln();

					if (highOffset - lowOffset <= 3)
					{
						// We are ready to break out - simply find which of the four points we have has the minimum
						// GCF
						unsigned long minGcf = min(min(min(lowGcf, lowGcfOption), highGcfOption), highGcf);

						// Set lowOffset as our best result; bias low
						if (lowGcf == minGcf)
						{
							bestOffset = lowOffset;
						}
						else if (lowGcfOption == minGcf)
						{
							bestOffset = lowOffsetOption;
						}
						else if (highGcfOption == minGcf)
						{
							bestOffset = highOffsetOption;
						}
						else if (highGcf == minGcf)
						{
							bestOffset = highOffset;
						}

						//DEFAULT_PRINT->print("#");
						//DEFAULT_PRINT->print(lowOffset);
						//Ln();

						// We're done
						break;
					}

					// Home in on the minimum. We want to keep the lowest point inside the extrema of our search.
					if (lowGcfOption < highGcfOption)
					{
						highOffset = highOffsetOption;
						highGcf = highGcfOption;
					}
					else
					{
						lowOffset = lowOffsetOption;
						lowGcf = lowGcfOption;
					}
				}

				break;
			}

			float result = (bestOffset != ~0)  ? GetFrequencyForOffsetFixed(bestOffset) : -2.0f;
			minFrequency = GetFrequencyForOffsetFixed(maxBestOffset);
			maxFrequency = GetFrequencyForOffsetFixed(minBestOffset);

			DEBUG_PRINT_STATEMENTS(
			{
				PrintStringFloat("result", result); Ln();
				PrintStringFloat("minFrequency", minFrequency); Ln();
				PrintStringFloat("maxFrequency", maxFrequency); Ln();
			});

			return result;
		}

		bool m_enableDetailedSearch;
		
		int m_minFrequency;
		int m_maxFrequency;
		int m_correlationDipThresholdPercent;
		int m_gcfStep;
		int m_baseOffsetStep;
		int m_baseOffsetStepIncrement;

	private:
		int m_audioPin;
	};

	Tuner()
#if ENABLE_LCD
		, m_lcd(2, 3, 4, 5, 6, 7, 8)
#endif // #if ENABLE_LCD
		: m_tunerNote(-1)
		, m_mode(TunerMode::Tuner)
	{
		m_a440.f = DEFAULT_A440;

		for (int i = 0; i < NUM_CHANNELS; ++i)
		{
			m_channels[i].SetPin(i);
		}

		DEBUG_PRINT_STATEMENTS(Serial.write("Constructing tuner..."); Ln(););

#if ENABLE_LCD
		g_lcd = &m_lcd;
#endif // #if ENABLE_LCD
	}

	~Tuner()
	{
#if ENABLE_LCD
		g_lcd = NULL;
#endif // #if ENABLE_LCD
	}

	void Start()
	{
		DEBUG_PRINT_STATEMENTS(Serial.write("Starting tuner..."); Ln(););

#if ENABLE_LCD
		m_lcd.clear();
#if ENABLE_STARTUP_MESSAGE
		m_lcd.print("Hello world");
		delay(2000);
#endif // #if ENABLE_STARTUP_MESSAGE
		m_lcd.clear();
		for (int i = 0; i < CHARACTER_WIDTH; ++i)
		{
			Glyph g;
			for (int j = 0; j < CHARACTER_HEIGHT; ++j)
			{
				g[j] = (1 << (CHARACTER_WIDTH - 1 - i));
			}
			m_lcd.setCharacterGlyph(i, g);
		}
		m_lcd.setCharacterGlyph(TICK_GLYPH, g_tickGlyph);
#endif // #if ENABLE_LCD

		// Enable auto-trigger enable
		sbi(ADCSRA, ADATE);
		// Set auto-trigger to free-running mode
		cbi(ADCSRB, ADTS0);
		cbi(ADCSRB, ADTS1);
		cbi(ADCSRB, ADTS2);

		ADMUX = (1 << 6);

		// Left-adjust result so we only have to read 8 bits
		sbi(ADMUX, ADLAR); // right-adjust for 8 bits

		// Setup the prescaler
		unsigned char adcsra = ADCSRA;
		adcsra = ((adcsra & 0xF8) | PRESCALER); // mask off / re-set prescaler bits
		ADCSRA = adcsra;

		// Disable the conversion complete interrupt so we can read the flag
		cbi(ADCSRA, ADIE); // Interrupt Enable

		// Start the shebang
		sbi(ADCSRA, ADSC);

#if ENABLE_LCD
		static int const MIN_MEMORY = 89;
		if (AvailableMemory() < MIN_MEMORY)
		{
			for(;;)
			{
				m_lcd.clear();
				m_lcd.print("OOM");
				m_lcd.setCursor(0, 1);
				m_lcd.print(AvailableMemory() - MIN_MEMORY);
				delay(2000);
			}
		}
#endif // #if ENABLE_LCD

		LoadTuning();
		//TunePitch();

		m_lastMicros = 0;
	}

	void Stop()
	{
		DEBUG_PRINT_STATEMENTS(Serial.write("Stopping tuner..."); Ln(););
		
		// Disable auto-trigger mode
		cbi(ADCSRA, ADATE);

		// Right-adjust result (default)
		cbi(ADMUX, ADLAR);
	}

	unsigned long LoadEepromLong(int address)
	{
		unsigned long result = 0;
		unsigned long t = 0; // to avoid spurious warning :/ 
		result |= EEPROM.read(address);
		t = EEPROM.read(address + 1);
		result |= (t << 8);
		t = EEPROM.read(address + 2);
		result |= (t << 16);
		t = EEPROM.read(address + 3);
		result |= (t << 24);
		return result;
	}

	void SaveEepromLong(int address, unsigned long i)
	{
		EEPROM.write(address, i & 0xFF);
		EEPROM.write(address + 1, (i >> 8) & 0xFF);
		EEPROM.write(address + 2, (i >> 16) & 0xFF);
		EEPROM.write(address + 3, (i >> 24) & 0xFF);
	}

	void SaveTuning()
	{
		SaveEepromLong(0, m_a440.ul);
	}

	void LoadTuning()
	{
		//Serial.println("Saving default tuning");
		m_a440.f = DEFAULT_A440; //@HACK
		SaveTuning();
		m_a440.ul = LoadEepromLong(0);
		//PrintStringFloat("Loaded test tuning", m_a440.f);

		//m_a440.ul = LoadEepromLong(0);
		m_a440.f = DEFAULT_A440; //@HACK
		if ((m_a440.f < ABSOLUTE_MIN_FREQUENCY) || (m_a440.f > ABSOLUTE_MAX_FREQUENCY))
		{
			DEBUG_PRINT_STATEMENTS(
			{
				PrintStringFloat("Bad tuning", m_a440.f); Ln();
				Serial.println("No saved tuning or corrupted value, loading default");
			});
			m_a440.f = DEFAULT_A440;
			SaveTuning();
		}
		DEBUG_PRINT_STATEMENTS(PrintStringFloat("Loaded tuning", m_a440.f); Ln(););
	}

	// Given a MIDI note index, returns the corresponding index into the global string array of note names.
	int GetNoteNameIndex(int note)
	{
		return (note + 3) % 12;
	}

	// Given a MIDI note index, get the name of the note as ASCII.
	const char* GetNoteName(int note)
	{
		if (note >= 0)
		{
			return g_noteNames[GetNoteNameIndex(note)];
		}
		else
		{
			return "  ";
		}
	}

#if (TEMPERAMENT == EQUAL_TEMPERAMENT)
	// Converts a fundamental frequency in Hz to a MIDI note index.  Slow.
	int GetMidiNoteIndexForFrequency(float frequency)
	{
		if (frequency < 0.0f)
		{
			return -1;
		}

		// Shift the note down half a semitone so the frequency interval that maps to a MIDI note is centered on the note.
		frequency *= QUARTERTONE_DOWN;

		int note = A440_NOTE;
		float a440 = m_a440.f;
		float a440_2 = 2.0f * a440;
		while (frequency < a440)
		{
			frequency *= 2.0f;
			note -= 12;
		}
		while (frequency > a440_2)
		{
			frequency *= 0.5f;
			note += 12;
		}
		while (frequency > a440)
		{
			frequency *= SEMITONE_DOWN;
			note += 1;
		}
		return note;
	}

	// Compute the fundamental frequency of a given MIDI note index.  Slow.
	float GetFrequencyForMidiNoteIndex(int note)
	{
		if (note < 0.0f)
		{
			return -1.0f;
		}

		float result = m_a440.f;
		
		while (note < A440_NOTE)
		{
			note += 12;
			result *= 0.5f;
		}
		
		while (note > A880_NOTE)
		{
			note -= 12;
			result *= 2.0f;
		}

		while (note > A440_NOTE)
		{
			--note;
			result *= SEMITONE_UP;
		}

		return result;
	}
#elif (TEMPERAMENT == JUST_TEMPERAMENT)
	// Converts a fundamental frequency in Hz to a MIDI note index.  Slow.
	int GetMidiNoteIndexForFrequency(float frequency)
	{
		DEBUG_PRINT_STATEMENTS(Serial.write("GetMidiNoteIndexForFrequency()"); Ln(););

		if (frequency <= 0.0f)
		{
			DEBUG_PRINT_STATEMENTS(Serial.write("earlying out, freq is <= 0"); Ln(););
			return -1;
		}

		// Shift the note down up a semitone so the frequency interval that maps to a MIDI note is centered on the note.
		// (Algorithm here is different from the one for equal temperament, we have to shift in the other direction.)
		frequency *= QUARTERTONE_UP;

		DEBUG_PRINT_STATEMENTS(PrintStringFloat("Frequency", frequency); Ln(););

		int note = A440_NOTE;
		float a440 = m_a440.f;
		float a440_2 = 2.0f * a440;
		DEBUG_PRINT_STATEMENTS(PrintStringFloat("a440", a440); Ln(););
		while (frequency < a440)
		{
			DEBUG_PRINT_STATEMENTS(Serial.write("Double up"); Ln(););
			frequency *= 2.0f;
			note -= 12;
		}
		while (frequency >= a440_2)
		{
			DEBUG_PRINT_STATEMENTS(Serial.write("Double down"); Ln(););
			frequency *= 0.5f;
			note += 12;
		}

		DEBUG_PRINT_STATEMENTS(PrintStringFloat("Centered frequency", frequency); Ln(););

		auto rangeIndex = 0;
		float rangeHigh = g_noteSemitoneRatio[rangeIndex] * a440;
		float rangeLow = -1.0f;
		for (;;)
		{
			rangeLow = rangeHigh;
			++rangeIndex;
			rangeHigh = g_noteSemitoneRatio[rangeIndex] * a440;
			DEBUG_PRINT_STATEMENTS(
			{
				PrintStringFloat("rangeLow", rangeLow); Ln();
				PrintStringFloat("rangeHigh", rangeHigh); Ln();
			});
			if ((frequency >= rangeLow) && (frequency < rangeHigh))
			{
				DEBUG_PRINT_STATEMENTS(PrintStringFloat("found note!", rangeHigh); Ln(););
				break;
			}
			++note;
			DEBUG_PRINT_STATEMENTS(PrintStringInt("note", note); Ln(););
		}
		DEBUG_PRINT_STATEMENTS(PrintStringInt("Final note", note); Ln(););
		return note;
	}

	// Compute the fundamental frequency of a given MIDI note index.  Slow.
	float GetFrequencyForMidiNoteIndex(int note)
	{
		DEBUG_PRINT_STATEMENTS( Serial.write("GetFrequencyForMidiNoteIndex()"); Ln(); );

		if (note < 0.0f)
		{
			return -1.0f;
		}

		float result = m_a440.f;

		while (note < A440_NOTE)
		{
			note += 12;
			result *= 0.5f;
		}

		while (note > A880_NOTE)
		{
			note -= 12;
			result *= 2.0f;
		}

		result *= g_noteSemitoneRatio[note - A440_NOTE];

		return result;
	}
#else
#error Unknown temperament!
#endif // #if EQUAL_TEMPERAMENT

	void RenderWideGlyphForNote(int note)
	{
#if ENABLE_LCD
		if (note >= 0)
		{
			int nameIndex = GetNoteNameIndex(note);
			int glyphIndex = g_noteGlyphIndex[nameIndex];
			m_lcd.setWideCharacterGlyph(NOTE_GLYPH, g_noteGlyphs[glyphIndex]);
			m_lcd.setCursor(1, 0); // DO NOT FACTOR THIS OUT - setting a character glyph sets the next write address; this sets it to DD RAM, whereas setWideCharacterGlyph will leave it in CGRAM
			m_lcd.write(NOTE_GLYPH);
			m_lcd.write(NOTE_GLYPH + 1);
			if (g_noteSharpSign[nameIndex])
			{
				m_lcd.write('#');
			}
			else
			{
				m_lcd.write(' ');
			}
		}
		else
		{
			m_lcd.setCursor(1, 0);
			m_lcd.print("   ");
		}
#else
		(void)note;
#endif // #if ENABLE_LCD
	}

	void PrintCenterTick()
	{
#if ENABLE_LCD
		m_lcd.setCursor(DISPLAY_WIDTH >> 1, 0);
		m_lcd.write(TICK_GLYPH);
#endif // #if ENABLE_LCD
	}

	void LcdTick(float f)
	{
#if ENABLE_LCD
		uint8_t tick = uint8_t(f * MAX_TICK);
		uint8_t x = (tick / 5) + 1;
		uint8_t glyph = tick % 5;
		if (x < DISPLAY_WIDTH)
		{
			m_lcd.setCursor(x, 1);
			m_lcd.write(glyph);
		}
		if ((x != m_lastLcdTickX) && (m_lastLcdTickX < DISPLAY_WIDTH))
		{
			m_lcd.setCursor(m_lastLcdTickX, 1);
			m_lcd.write(' ');
		}
		m_lastLcdTickX = x;
#else
		(void)f;
#endif // #if ENABLE_LCD
	}

	void TunePitch()
	{
		unsigned long lastPress = millis();
		bool firstTime = true;
		while (millis() - lastPress < 2000)
		{
			g_pitchDownButton.Update();
			g_pitchUpButton.Update();

			bool buttonPressed = firstTime;
			if (g_pitchDownButton.JustPressed())
			{
				m_a440.f -= 1.0f;
				buttonPressed = true;
			}

			if (g_pitchUpButton.JustPressed())
			{
				m_a440.f += 1.0f;
				buttonPressed = true;
			}

			if (m_a440.f < ABSOLUTE_MIN_FREQUENCY)
			{
				m_a440.f = ABSOLUTE_MIN_FREQUENCY;
			}

			if (m_a440.f > ABSOLUTE_MAX_FREQUENCY)
			{
				m_a440.f = ABSOLUTE_MAX_FREQUENCY;
			}

			if (buttonPressed)
			{
				lastPress = millis();
#if ENABLE_LCD
				m_lcd.clear();
				m_lcd.print("A = ");
				PrintFloat(m_a440.f, 2);
				m_lcd.print(" Hz   ");
#endif // #if ENABLE_LCD
			}

			firstTime = false;
		}
#if ENABLE_LCD
		m_lcd.clear();
#endif // #if ENABLE_LCD
		SaveTuning();
	}

	void CycleModes()
	{
		m_mode = static_cast<TunerMode::Type>((m_mode + 1) % TunerMode::Max);
	}

	void Go()
	{
		DEBUG_PRINT_STATEMENTS(Serial.write("Initializing..."); Ln(););

		//{
		//	DEFAULT_PRINT->print("#");
		//	float maxFreq = m_channels[0].GetFrequencyForOffsetFixed(I2FIXED(ABSOLUTE_MIN_SAMPLES));
		//	float secondLastFreq = m_channels[0].GetFrequencyForOffsetFixed(I2FIXED(ABSOLUTE_MIN_SAMPLES) + 1);
		//	PrintStringFloat("Max freq", maxFreq);
		//	PrintStringFloat("Second last freq", secondLastFreq);
		//	PrintStringFloat("Min freq discr", maxFreq - secondLastFreq);
		//	Ln();
		//}

#define TEST_FREQUENCY(x) //PrintStringInt(#x, GetMidiNoteIndexForFrequency(x)); Ln();
		TEST_FREQUENCY(474.83380f);
		TEST_FREQUENCY(500.0f);
		TEST_FREQUENCY(600.0f);
		TEST_FREQUENCY(700.0f);
		TEST_FREQUENCY(800.0f);
		TEST_FREQUENCY(900.0f);

		Start();

#if PRINT_FREQUENCY_TO_SERIAL_VT100
		ClearScreen();
#endif // #if PRINT_FREQUENCY_TO_SERIAL_VT100

		float filteredFrequency = -1.0f;

		int activeChannelIndex = 0;

		while(1)
		{
			if (Serial.available())
			{
				while (Serial.available())
				{
					//DEFAULT_PRINT->print("#"); DEFAULT_PRINT->print(Serial.available()); Ln();
					char command = Serial.read();
					char peek = Serial.peek();
					DEFAULT_PRINT->print("#read command: "); DEFAULT_PRINT->print(command); Ln();
					DEFAULT_PRINT->print("#next character: "); DEFAULT_PRINT->print(peek); Ln();
					switch (command)
					{
					case 'I': g_dumpOnNullReading = true; break;
					case 'i': g_dumpOnNullReading = false; break;
					case 'f': g_dumpBelowFrequency = Serial.parseInt(); break;
					case 'c': activeChannelIndex = Serial.parseInt(); break;
					case 'm': m_channels[activeChannelIndex].m_minFrequency = max(Serial.parseInt(), ABSOLUTE_MIN_FREQUENCY); break;
					case 'M': m_channels[activeChannelIndex].m_maxFrequency = min(Serial.parseInt(), ABSOLUTE_MAX_FREQUENCY); break;
					case 'p': m_channels[activeChannelIndex].m_correlationDipThresholdPercent = Serial.parseInt(); break;
					case 'g': m_channels[activeChannelIndex].m_gcfStep = max(Serial.parseInt(), 1); break;
					case 'o': m_channels[activeChannelIndex].m_baseOffsetStep = max(Serial.parseInt(), 1); break;
					case 's': m_channels[activeChannelIndex].m_baseOffsetStepIncrement = Serial.parseInt(); break;
					case 'x': m_channels[activeChannelIndex].m_enableDetailedSearch = (Serial.parseInt() != 0); break;
					case 'd': g_dumpMode = Serial.parseInt(); break;
					case 'e':
					{
						int i = Serial.parseInt();
						DEFAULT_PRINT->print("e");
						DEFAULT_PRINT->print(i);
						Ln();
					}
					break;
					case '\n': break;
					}

					DEFAULT_PRINT->print("#");
					DEFAULT_PRINT->print(command); DEFAULT_PRINT->print(COMMA_SEPARATOR);
					DEFAULT_PRINT->print(activeChannelIndex); DEFAULT_PRINT->print(COMMA_SEPARATOR);
					DEFAULT_PRINT->print(m_channels[activeChannelIndex].m_minFrequency); DEFAULT_PRINT->print(COMMA_SEPARATOR);
					DEFAULT_PRINT->print(m_channels[activeChannelIndex].m_maxFrequency); DEFAULT_PRINT->print(COMMA_SEPARATOR);
					DEFAULT_PRINT->print(m_channels[activeChannelIndex].m_correlationDipThresholdPercent); DEFAULT_PRINT->print(COMMA_SEPARATOR);
					DEFAULT_PRINT->print(m_channels[activeChannelIndex].m_gcfStep); DEFAULT_PRINT->print(COMMA_SEPARATOR);
					DEFAULT_PRINT->print(m_channels[activeChannelIndex].m_baseOffsetStep); DEFAULT_PRINT->print(COMMA_SEPARATOR);
					DEFAULT_PRINT->print(m_channels[activeChannelIndex].m_baseOffsetStepIncrement);
					Ln();

					// Indicate that we're ready for a command
					DEFAULT_PRINT->print("c"); Ln();

					// Give a little time to react in case the host wants to send multiple commands in a row
					delay(25);
				}
			}
			else
			{
				// Nothing received - we are ready for more
				DEFAULT_PRINT->print("c"); Ln();
			}

			//DEFAULT_PRINT->print("#Left input loop"); Ln();

			DEBUG_PRINT_STATEMENTS(Serial.write("Main tuner loop"); Ln(););
			
			DEBUG_PRINT_STATEMENTS(Serial.write("Button updates"); Ln(););
			g_pitchDownButton.Update();
			g_pitchUpButton.Update();

			if (g_pitchDownButton.IsPressed() || g_pitchUpButton.IsPressed())
			{
				TunePitch();
			}

			g_modeButton.Update();

			if (g_modeButton.JustPressed())
			{
				DEBUG_PRINT_STATEMENTS(Serial.write("Mode button pressed"); Ln(););

				// Exit current mode
				switch (m_mode)
				{
				default: break;
				}

				// Cycle modes
				CycleModes();
				
				// Enter new mode
#if ENABLE_LCD
				m_lcd.clear();
#endif // #if ENABLE_LCD

				switch (m_mode)
				{
				default: 
					break;
				}
			}

#if ENABLE_LCD
			m_lcd.home();
#endif // #if ENABLE_LCD

			DEBUG_PRINT_STATEMENTS(Serial.write("DetermineSignalPitch"); Ln(););
			//Fixed bestOffset;
			//int numCoarseCorrelations = 0;
			//int numFineCorrelations = 0;
			//s_correlationStep = 2;
			//float instantFrequency = m_channels[0].DetermineSignalPitch(bestOffset, numCoarseCorrelations, numFineCorrelations);
			//Fixed bestOffset2;
			//float instantFrequency2;
			//s_correlationStep = 32;
			//instantFrequency2 = m_channels[0].DetermineSignalPitch(bestOffset2, numCoarseCorrelations, numFineCorrelations);
			//Fixed bestOffset3;
			//float instantFrequency3;
			//s_correlationStep = 64;
			//instantFrequency3 = m_channels[0].DetermineSignalPitch(bestOffset3, numCoarseCorrelations, numFineCorrelations);
			//Fixed bestOffset4;
			//float instantFrequency4;
			//s_correlationStep = 128;
			//instantFrequency4 = m_channels[0].DetermineSignalPitch(bestOffset4, numCoarseCorrelations, numFineCorrelations);
			float minSignalFrequency = -1.0f;
			float maxSignalFrequency = -1.0f;
            int signalMin = 0;
            int signalMax = 0;
			float instantFrequency = m_channels[0].DetermineSignalPitch(minSignalFrequency, maxSignalFrequency, signalMin, signalMax);
#if FAKE_FREQUENCY
			static float t = 0.0f;
			t += 0.001f;
			instantFrequency = 400.0f + 200.0f * sinf(t);
#endif // #if FAKE_FREQUENCY
			DEBUG_PRINT_STATEMENTS(PrintStringFloat("instantFrequency", instantFrequency); Ln(););

			DEBUG_PRINT_STATEMENTS(Serial.write("GetMidiNoteIndexForFrequency"); Ln(););
			m_tunerNote = GetMidiNoteIndexForFrequency(instantFrequency);
			float centerDisplayFrequency = GetFrequencyForMidiNoteIndex(m_tunerNote);
			float minDisplayFrequency = centerDisplayFrequency * FREQUENCY_FILTER_WINDOW_RATIO_DOWN;
			float maxDisplayFrequency = centerDisplayFrequency * FREQUENCY_FILTER_WINDOW_RATIO_UP;

			DEBUG_PRINT_STATEMENTS(Serial.write("instantFrequency"); Ln(););
			if (instantFrequency >= 0.0f)
			{
				if ((filteredFrequency >= 0.0f) && (filteredFrequency >= minDisplayFrequency) && (filteredFrequency <= maxDisplayFrequency))
				{
					static float const RATE = 0.1f;
					filteredFrequency = (1.0f - RATE) * filteredFrequency + (RATE * instantFrequency);
				}
				else
				{
					filteredFrequency = instantFrequency;
				}
			}
			else
			{
				filteredFrequency = -1.0f;
			}

//#if ENABLE_LCD || PRINT_FREQUENCY_TO_SERIAL || PRINT_FREQUENCY_TO_SERIAL_VT100
//			DEBUG_PRINT_STATEMENTS(Serial.write("percent"); Ln(););
//			float percent = 0.0f;
//			if (filteredFrequency < centerDisplayFrequency)
//			{
//				percent = 0.5f * (filteredFrequency - minDisplayFrequency) / (centerDisplayFrequency - minDisplayFrequency);
//			}
//			else
//			{
//				percent = 0.5f + 0.5f * (filteredFrequency - centerDisplayFrequency) / (maxDisplayFrequency - centerDisplayFrequency);
//			}
//#endif // #if ENABLE_LCD || PRINT_FREQUENCY_TO_SERIAL

#if ENABLE_LCD
			RenderWideGlyphForNote(m_tunerNote);

			switch(m_mode)
			{
			case TunerMode::Tuner:
				{
					PrintCenterTick();
					if (filteredFrequency > 0.0f)
					{
						LcdTick(percent);
					}
					else
					{
						LcdTick(-1);
					}
				}
				break;
			default:
				break;
			}
#endif // #if ENABLE_LCD

			unsigned long nowMicros = micros();
			//unsigned long deltaMicros = nowMicros - m_lastMicros;

			m_lastMicros = nowMicros;

#if PRINT_FREQUENCY_TO_SERIAL
#if PRINT_FREQUENCY_TO_SERIAL_VT100
			MoveCursor(0, 0);
			Serial.print("\x1B[0m");
#endif // #if PRINT_FREQUENCY_TO_SERIAL_VT100
			//Serial.print("-----"); Ln();
            //float sps = 1000000.0f / deltaMicros;
            //PrintStringFloat("SPS ", sps); Ln();
			//PrintStringInt("ofs (int)", bestOffset); Ln();
			//PrintStringFloat("ofs (float)", FIXED2F(bestOffset)); Ln();
			//PrintStringInt("numCoarseCorrelations", numCoarseCorrelations); DEFAULT_PRINT->print("   "); Ln();
			//PrintStringInt("numFineCorrelations", numFineCorrelations); DEFAULT_PRINT->print("   "); Ln();
			//PrintStringFloat("Instant freq", instantFrequency); Ln();
			DEFAULT_PRINT->print('r');
			PrintFloat(instantFrequency); Serial.print(COMMA_SEPARATOR);
            PrintFloat(minSignalFrequency); Serial.print(COMMA_SEPARATOR);
            PrintFloat(maxSignalFrequency); Serial.print(COMMA_SEPARATOR);
            Serial.print(signalMin); Serial.print(COMMA_SEPARATOR);
            Serial.print(signalMax); Ln();
			//Serial.print("#Line 1"); Ln();
			//Serial.print("#Line 2"); Ln();
			//Serial.print("#Line 3"); Ln();
			//PrintStringInt("ofs2", bestOffset2); Ln();
			//PrintStringFloat("Instant freq 2", instantFrequency2); Ln();
			//PrintStringInt("ofs3", bestOffset3); Ln();
			//PrintStringFloat("Instant freq 3", instantFrequency3); Ln();
			//PrintStringInt("ofs4", bestOffset4); Ln();
			//PrintStringFloat("Instant freq 4", instantFrequency4); Ln();
			//PrintStringFloat("Filtered freq", filteredFrequency); Ln();
			//PrintStringInt("MIDI note", m_tunerNote); Ln();
			//PrintStringFloat("MIDI frequency", centerFrequency); Ln();
			//PrintStringFloat("Bottom frequency for note", minFrequency); Ln();
			//PrintStringFloat("Top frequency for note", maxFrequency); Ln();
			//PrintStringFloat("Percent note (50% is perfect)", percent); Ln();
//#if PRINT_FREQUENCY_TO_SERIAL_VT100
//			percent = Clamp(percent, 0.0f, 0.9999f);
//			const int numCharacters = 31;
//			const int centerCharacterIndex = numCharacters / 2;
//			int characterIndex = (percent * numCharacters);
//			const char* tunerCharacters[2][3] = {{"»", /*"·"*/"oCOMMA_SEPARATOR«"}, {"|COMMA_SEPARATOROCOMMA_SEPARATOR|"}};
//			for (int i = 0; i < numCharacters; ++i)
//			{
//				int leftRightSelector = sgn(i - centerCharacterIndex) + 1;
//				int onOffSelector = (i == characterIndex) ? 1 : 0;
//				Serial.print(tunerCharacters[onOffSelector][leftRightSelector]);
//			}
//			Serial.print(" ");
//			Ln();
//#endif // #if PRINT_FREQUENCY_TO_SERIAL_VT100
#endif //#if PRINT_FREQUENCY_TO_SERIAL
		}

		Stop();
	}

private:
#if ENABLE_LCD
	LiquidCrystal m_lcd;
#endif // #if ENABLE_LCD
	int m_lastLcdTickX;
	
	union
	{
		unsigned long ul;
		float f;
	} m_a440;
	STATIC_ASSERT(sizeof(unsigned long) == sizeof(float)); // verify that things match up for the above union
	
	int m_tunerNote;
	TunerMode::Type m_mode;

	Channel m_channels[NUM_CHANNELS];
	unsigned long m_lastMicros;
};

//int Tuner::s_correlationStep = 2;


///////////////////////////////////////////////////////////////////////////////
// Main stuff
///////////////////////////////////////////////////////////////////////////////

void setup()                                        // run once, when the sketch starts
{
	Serial.begin(115200); // for debugging
	//Serial.println("Setup hardwareserial.");
	pinMode(kStatusPin, OUTPUT);            // sets the digital pin as output
	pinMode(kStatusPin2, OUTPUT);            // sets the digital pin as output
	digitalWrite(kStatusPin, LOW); 
	digitalWrite(kStatusPin2, LOW);
}

void loop()
{
	Tuner tuner;
	tuner.Go();
} 
