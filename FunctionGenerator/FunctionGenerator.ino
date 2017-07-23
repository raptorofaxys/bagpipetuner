//#define DEFAULT_PRINT (&Serial)
//
//void Ln(Print* p = DEFAULT_PRINT)
//{
//	p->println("");
//}
//
//void ClearScreen(Print* p = DEFAULT_PRINT)
//{
//	p->print("\x1B[2J");
//}
//
//void MoveCursor(int x, int y, Print* p = DEFAULT_PRINT)
//{
//	p->print("\x1B[");
//	p->print(y + 1);
//	p->print(";");
//	p->print(x + 1);
//	p->print("H");
//}
//
//void Space(Print* p = DEFAULT_PRINT)
//{
//	p->print(" ");
//}
//
//void Cls(Print* p = DEFAULT_PRINT)
//{
//	p->print(char(27));
//	p->print("[2J");
//}
//
//void PrintFloat(float f, int decimals = 10, Print* p = DEFAULT_PRINT)
//{
//	if (f < 0)
//	{
//		p->print("-");
//		f = -f;
//	}
//	else
//	{
//		p->print(" ");
//	}
//
//	int b = int(f);
//	p->print(b);
//	p->print(".");
//	f -= b;
//	for (int i = 0; i < decimals; ++i)
//	{
//		f *= 10.0f;
//		int a = int(f);
//		p->print(a);
//		f -= a;
//	}
//}
//
//void PrintHex(int h, Print* p = DEFAULT_PRINT)
//{
//	static char const* hex = "0123456789ABCDEF";
//	p->print(hex[(h & 0xF000) >> 12]);
//	p->print(hex[(h & 0x0F00) >> 8]);
//	p->print(hex[(h & 0x00F0) >> 4]);
//	p->print(hex[(h & 0x000F) >> 0]);
//}
//
//void PrintStringInt(char const* s, int v, Print* p = DEFAULT_PRINT)
//{
//	p->print(s);
//	p->print(": ");
//	p->print(v);
//}
//
//void PrintStringLong(char const* s, long v, Print* p = DEFAULT_PRINT)
//{
//	p->print(s);
//	p->print(": ");
//	p->print(v);
//}
//
//void PrintStringFloat(char const* s, float f, int decimals = 5, Print* p = DEFAULT_PRINT)
//{
//	p->print(s);
//	p->print(": ");
//	PrintFloat(f, decimals);
//}

void setup()
{
	//Serial.begin(115200);
}

unsigned long g_nowUs = 0;
unsigned long g_lastUs = 0;
unsigned long g_currentDeltaUs = 0;

#if 1
#define DISASM_NOINLINE 
#else
#define DISASM_NOINLINE __attribute__((noinline))
#endif

class FunctionGenerator
{
public:
	FunctionGenerator(int inputPin, int outputPin)
		: m_analogReadDelay(outputPin) // round-robin the analogwrites
		, m_inputPin(inputPin)
		, m_outputPin(outputPin)
		, m_lastUs(0)
		, m_toggleDeltaUs(0)
		, m_toggleUsAccumulator(0)
		, m_currentValue(false)
	{
		pinMode(inputPin, INPUT);
		pinMode(outputPin, OUTPUT);

		// Adapted from wiring_digital.c in the Arduino core
		m_bit = digitalPinToBitMask(outputPin);
		m_out = portOutputRegister(digitalPinToPort(outputPin));
	}

	void DISASM_NOINLINE Update()
	{
		m_toggleUsAccumulator += g_currentDeltaUs;

		--m_analogReadDelay;
		if (m_analogReadDelay <= 0)
		{
			m_analogReadDelay = 65000;
			int inValue = analogRead(m_inputPin);
			int frequency = map(inValue, 0, 1023, 40, 1000);

			// one million us in a second, two toggles per cycle
			m_toggleDeltaUs = 500000UL / frequency;
		}

		if (m_toggleUsAccumulator > m_toggleDeltaUs)
		{
			m_toggleUsAccumulator -= m_toggleDeltaUs;
			m_currentValue = !m_currentValue;
			fastDigitalWrite(m_currentValue);
		}
	}

private:
	
	// Adapted from wiring_digital.c in the Arduino core
	void DISASM_NOINLINE fastDigitalWrite(uint8_t val)
	{
		uint8_t oldSREG = SREG;
		cli();

		if (val == LOW)
		{
			*m_out &= ~m_bit;
		}
		else
		{
			*m_out |= m_bit;
		}

		SREG = oldSREG;
	}

	unsigned int m_analogReadDelay;
	int m_inputPin;
	int m_outputPin;
	unsigned long m_lastUs;
	unsigned int m_toggleDeltaUs;
	unsigned int m_toggleUsAccumulator;
	bool m_currentValue;

	volatile uint8_t *m_out;
	uint8_t m_bit;
};

FunctionGenerator f1(A0, 2);
FunctionGenerator f2(A1, 3);
FunctionGenerator f3(A2, 4);
FunctionGenerator f4(A3, 5);

void loop()
{
	for (;;)
	{
		g_nowUs = micros();
		g_currentDeltaUs = g_nowUs - g_lastUs;
		g_lastUs = g_nowUs;

		f1.Update();
		f2.Update();
		f3.Update();
		f4.Update();
	}
}