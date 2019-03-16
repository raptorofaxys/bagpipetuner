/////////////////////////////
// Fixed-point math
/////////////////////////////

typedef int Fixed;
typedef long DoubleFixed;
static int const FIXED_SHIFT = 6; // @TODO: can we bump this up to 6? where is the issue? (2*6=12 => 4 bits left for integer portion during a multiply)
static int const FIXED_ONE = (1 << FIXED_SHIFT);
static int const FRAC_MASK = (1 << FIXED_SHIFT) - 1;
#define FIXED_INT(x) int(x >> FIXED_SHIFT)
#define FIXED_FRAC(x) int(x & FRAC_MASK)
#define		I2FIXED(x) ((Fixed) ((x) << FIXED_SHIFT))
#define		F2FIXED(x) ((Fixed) ((x) * (1 << FIXED_SHIFT)))
#define		FIXED2I(x) ((x) >> FIXED_SHIFT)
#define		FIXED2F(x) ((x) / float(1 << FIXED_SHIFT))
static int const PRIME_SHIFT = 8;

/////////////////////////////
// EEPROM Helpers
/////////////////////////////
typedef union
{
    unsigned long ul;
    float f;
} FourByteUnion;
STATIC_ASSERT(sizeof(unsigned long) == sizeof(float)); // verify that things match up for the above union

/////////////////////////////
// Global tuner precision configuration
/////////////////////////////
static int const PRESCALER = 0b00000101;
static int const PRESCALER_DIVIDE = (1 << PRESCALER);
static int const ADC_CLOCKS_PER_ADC_CONVERSION = 13;
static unsigned long const CPU_CYCLES_PER_SAMPLE = ADC_CLOCKS_PER_ADC_CONVERSION * PRESCALER_DIVIDE;
static unsigned long const SAMPLES_PER_SECOND = F_CPU / CPU_CYCLES_PER_SAMPLE;

static int const ABSOLUTE_MIN_FREQUENCY = 76;
static int const ABSOLUTE_MAX_FREQUENCY = 1100;
static int const ABSOLUTE_MIN_SAMPLES = SAMPLES_PER_SECOND / ABSOLUTE_MAX_FREQUENCY;
static int const ABSOLUTE_MAX_SAMPLES = SAMPLES_PER_SECOND / ABSOLUTE_MIN_FREQUENCY;
static int const MAX_BUFFER_SIZE = 2 * ABSOLUTE_MAX_SAMPLES + 1; // for interpolation

STATIC_ASSERT((SAMPLES_PER_SECOND / 2) > ABSOLUTE_MAX_FREQUENCY);
//STATIC_ASSERT(ABSOLUTE_MAX_SAMPLES <= 255); // so we can use unsigned chars as offsets; this is not the case with a prescaler of 101b and a minimum frequency of 75 Hz

/////////////////////////////
// Recording buffer
/////////////////////////////

// This buffer is now global because we need to the compiler to use faster addressing modes that are only available with
// fixed memory addresses. The buffer is now shared between the different tuner channels.
char g_recordingBuffer[MAX_BUFFER_SIZE];

/////////////////////////////
// GCF debugging helpers
/////////////////////////////
bool g_dumpOnLowAmplitude = false;
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
