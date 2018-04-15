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