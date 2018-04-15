#pragma once

#define ARRAY_COUNT(x) (sizeof(x) / sizeof(x[0]))

#define assert(x) if (!(x)) { Halt(__LINE__); }
#define HALT() Halt(__LINE__)
inline void Halt(int line)
{
	Serial.print("Halt: L");
	Serial.println(line);
	for (;;) {}
}

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

const int INT_MIN = (1 << (sizeof(int) * 8 - 1));
const int INT_MAX = (~INT_MIN);

// From http://www.arduino.cc/playground/Code/AvailableMemory
// This function will return the number of bytes currently free in RAM.
// written by David A. Mellis
// based on code by Rob Faludi http://www.faludi.com
// (reformatted for clarity)
// Note: I believe this does not work on recent versions of the Arduino environment.
// See the webpage above for alternatives.
inline int AvailableMemory()
{
    int size = 1024;
    byte* buf;

    while ((buf = (byte *)malloc(--size)) == NULL)
        ;

    free(buf);

    return size;
}
