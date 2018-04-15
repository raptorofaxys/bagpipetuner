#pragma once

#include "tuner_print.h"

static unsigned long const kTimer0Prescaler = 64; // see wiring.c
extern unsigned long volatile timer0_overflow_count;
class HighResTimer
{
public:
    void Start()
    {
        m_startTimer0 = TCNT0;
        m_startOverflowTimer0 = timer0_overflow_count; // see wiring.c
    }

    void Stop()
    {
        m_stopTimer0 = TCNT0;
        m_stopOverflowTimer0 = timer0_overflow_count; // see wiring.c
    }

    unsigned long GetCycles()
    {
        // we don't handle overflow wraparound; neither does millis(); would wrap around after approx. 51 days (according to my quite possibly erroneous calculations)
        return
            (m_stopTimer0 - m_startTimer0) * kTimer0Prescaler
            + (m_stopOverflowTimer0 - m_startOverflowTimer0) * 256 * kTimer0Prescaler;
    }

    float GetSeconds()
    {
        return float(GetCycles()) / F_CPU;
    }

    //void PrintDebug()
    //{
    //	PrintStringInt("this", (unsigned long) (this));
    //	PrintStringInt("a0", m_startTimer0);
    //	PrintStringInt(" b0", m_stopTimer0);
    //	PrintStringInt(" ao0", m_startOverflowTimer0);
    //	PrintStringInt(" ab0", m_stopOverflowTimer0);
    //}

    //private:
    unsigned char m_startTimer0;
    unsigned char m_stopTimer0;
    unsigned long m_startOverflowTimer0;
    unsigned long m_stopOverflowTimer0;
};

class ScopeProfiler
{
public:
    ScopeProfiler(const char* name, int times)
        : m_name(name)
        , m_line(-1)
        , m_times(times)
    {
        m_timer.Start();
    }

    ScopeProfiler(int line, int times)
        : m_name(NULL)
        , m_line(line)
        , m_times(times)
    {
        m_timer.Start();
    }

    ~ScopeProfiler()
    {
        m_timer.Stop();
        if (m_name)
        {
            Serial.print(m_name);
        }
        else
        {
            Serial.print('L');
            Serial.print(m_line);
        }
        Serial.print(": ");
        PrintFloat(static_cast<float>(m_timer.GetCycles()) / m_times);
        Serial.print("c / ");
        PrintFloat(static_cast<float>(m_timer.GetSeconds()) / m_times, 10);
        Serial.print("s ");
        //m_timer.PrintDebug();
        Ln();
    }
    //private:
    const char* m_name;
    int m_line;
    HighResTimer m_timer;
    int m_times;
};
#define PROFILE_COUNT 3000
#define PROFILE_STATEMENT(x) { ScopeProfiler t(#x, PROFILE_COUNT); for (uint16_t i = 0; i < PROFILE_COUNT; ++i) { x; } }
#define PROFILE_STATEMENT_LINE(x) { ScopeProfiler t(__LINE__, PROFILE_COUNT); for (uint16_t i = 0; i < PROFILE_COUNT; ++i) { x; } }
#define PROFILE_STATEMENT_LINE_NOP(x)
