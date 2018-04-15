#define EQUAL_TEMPERAMENT 1
#define JUST_TEMPERAMENT 2
#define TEMPERAMENT JUST_TEMPERAMENT

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

#if (TEMPERAMENT == EQUAL_TEMPERAMENT)
// Converts a fundamental frequency in Hz to a MIDI note index.  Slow.
int GetMidiNoteIndexForFrequency(float frequency, float a440)
{
    if (frequency < 0.0f)
    {
        return -1;
    }

    // Shift the note down half a semitone so the frequency interval that maps to a MIDI note is centered on the note.
    frequency *= QUARTERTONE_DOWN;

    int note = A440_NOTE;
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
float GetFrequencyForMidiNoteIndex(int note, float a440)
{
    if (note < 0.0f)
    {
        return -1.0f;
    }

    float result = a440;

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
int GetMidiNoteIndexForFrequency(float frequency, float a440)
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
float GetFrequencyForMidiNoteIndex(int note, float a440)
{
    DEBUG_PRINT_STATEMENTS(Serial.write("GetFrequencyForMidiNoteIndex()"); Ln(); );

    if (note < 0.0f)
    {
        return -1.0f;
    }

    float result = a440;

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