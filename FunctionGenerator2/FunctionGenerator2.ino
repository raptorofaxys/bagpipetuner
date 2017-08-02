#include <avr/pgmspace.h>
#include "wiring_private.h" // for sbi/cbi

#define DEFAULT_PRINT (&Serial)

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
	PrintFloat(f, decimals);
}

const unsigned int PRESCALER_DIVIDER = 64;

void setup()
{
	// maybe ask Arduino to setup the full pin for analogWrite for us, then simply tweak timer - actually, not needed
	// TCRR0B affects millis() and delay()
		
	// Reset and hold all timers
	GTCCR = 0b10000011;

	// Set pin 1 of B register as out - OC2A. This is Arduino digital pin 9.
	DDRB = 0b0000010;
	PORTB = 0; // tri-state all port B pins other than the above pins

	// Toggle OC1B on compare match (I think this could be disabled eventually - I think the ADC does not read the output port but the internal compare match signal),
	// but for now I will use this as a way to monitor sampling timing on an oscilloscope.
	// Clear timer on compare (CTC) against OCR1A;
	TCCR1A = 0b01010000;
	//TCCR1A = 0b01000000;
	TCCR1B = 0b00001011; // prescaler selected by the bottom three bits (1/256 currently)

	OCR1AH = 0;
	OCR1AL = 46; // ~170Hz

	// Disable all timer1 interrupts
	TIMSK1 = 0;

	// Use synchronous clocks
	ASSR = 0;

	// Release timers!
	GTCCR = 0;

	//pinMode(g_inputPin, INPUT);
	pinMode(7, INPUT_PULLUP);

	Serial.begin(115200);
	Serial.print("Starting up!"); Ln();
}

void loop()
{
	//unsigned int tuningTimer = 0;
	unsigned int lastIn = 0;
	for (;;)
	{
		unsigned int inValue = analogRead(A0);

		const int TUNING_THRESHOLD = 5;

		//unsigned int deltaIn = abs(inValue - lastIn);
		//if (((tuningTimer == 0) && (deltaIn > TUNING_THRESHOLD))
			//|| (tuningTimer > 0))
		if (digitalRead(7) == LOW)
		{
			unsigned int frequency = map(inValue, 0, 1023, 80, 1100);
			unsigned int timerCompare = (F_CPU / PRESCALER_DIVIDER / 2) / frequency - 1;
			unsigned char high = static_cast<unsigned char>(timerCompare >> 8);
			unsigned char low = static_cast<unsigned char>(timerCompare & 0xFF);

			//unsigned char sreg = SREG;
			//cli();
			//if (TCNT1 > timerCompare)
			//{
			//	TCNT1 = 0;
			//}
			//SREG = sreg;

			if (abs(inValue - lastIn) > TUNING_THRESHOLD)
			{
				//PrintStringInt("inValue", inValue); Ln();
				PrintStringInt("frequency", frequency); Ln();
				PrintStringInt("timerCompare", timerCompare); Ln();
				PrintStringInt("actual frequency", (F_CPU / PRESCALER_DIVIDER / 2) / timerCompare); Ln();
				//PrintStringInt("high", high); Ln();
				//PrintStringInt("low", low); Ln();

				//GTCCR = 0b10000011;
				unsigned char sreg = SREG;
				cli();
				if ((timerCompare < OCR1A) && (TCNT1 >= timerCompare))
				{
					TCNT1 = 0;
				}
				OCR1A = timerCompare;
				SREG = sreg;
				//OCR1AH = high;
				//OCR1AL = low;
				//GTCCR = 0;

				lastIn = inValue;
			}
			//PrintStringInt("TCNT1", TCNT1);
		}
	}
}