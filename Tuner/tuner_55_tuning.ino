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