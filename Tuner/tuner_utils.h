#pragma once

/////////////////////////////
// General utilities
/////////////////////////////

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
template<unsigned long I> struct PrintN;

// This is a very naive way of writing a static assertion facility, but avr-gcc is *very* far from compliant - it
// will allow all sorts of illegal constructs - and my patience is running out. :P  Even though this will generate
// a bit of code, it can hopefully be trivially stripped.
#define STATIC_ASSERT3(x, line) void func##line(static_assert_<x>) { }
#define STATIC_ASSERT2(x, line) STATIC_ASSERT3(x, line)
#define STATIC_ASSERT(x) STATIC_ASSERT2((x), __LINE__)
template <bool N> struct static_assert_;
template<> struct static_assert_<true> {};

/////////////////////////////
// Math utilities
/////////////////////////////

float Clamp(float v, float min_, float max_)
{
    return min(max(v, min_), max_);
}

// from http://stackoverflow.com/questions/1903954/is-there-a-standard-sign-function-signum-sgn-in-c-c
template <typename T> int sgn(T val)
{
    return (T(0) < val) - (val < T(0));
}

const int INT_MIN = (1 << (sizeof(int) * 8 - 1));
const int INT_MAX = (~INT_MIN);

/////////////////////////////
// ATmega utilities
/////////////////////////////

// from wiring_private.h
#ifndef cbi
#define cbi(sfr, bit) (_SFR_BYTE(sfr) &= ~_BV(bit))
#endif
#ifndef sbi
#define sbi(sfr, bit) (_SFR_BYTE(sfr) |= _BV(bit))
#endif 