/*

Arduino chromatic tuner.
2008-2018 Frederic Hamel

This version of the code is meant for a bagpipe tuner designed by Matthew Beall. Compared to the original version:
-it supports multiple input channels with channel-specific settings
-it much lower latency
-it has about double the precision over 2-3x the frequency range
-it has no support for LCD or MIDI mode.

Note that the code organization of this project is not so great - lots of non-inline functions in headers and stuff -
because it grew organically from waaaaay back when Arduino didn't support multi-file compilation. I haven't yet gone
back to reorganize everything in a modern way.

This is released under the Attribution-NonCommercial 3.0 Unported Creative Commons license:
http://creativecommons.org/licenses/by-nc/3.0/

See the original project writeup:
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
#include <inttypes.h>
#include "Print.h"

#include "tuner_utils.h"

#define ENABLE_STARTUP_MESSAGE 0
#define ENABLE_BUTTON_INPUTS 0

#define ENABLE_DEBUG_PRINT_STATEMENTS 0
#if ENABLE_DEBUG_PRINT_STATEMENTS
#define DEBUG_PRINT_STATEMENTS(x) x
#else
#define DEBUG_PRINT_STATEMENTS(x)
#endif

const char* COMMA_SEPARATOR = ", ";

static int const kStatusPin = 13;
static int const kStatusPin2 = 12;
static int const kPitchDownButtonPin = 9;
static int const kPitchUpButtonPin = 10;

#define DEFAULT_PRINT (&Serial)

const int kDebounceMs = 50;

// The code for the tuner is scattered among the .ino files in this folder. Since function prototype generation didn't
// appear to work in many cases, as a workaround (and to avoid having to reorganize everything in proper C++ with
// headers, forward declarations and such), I relied on the behaviour documented here:
// https://github.com/arduino/Arduino/wiki/Build-Process
// Which states:
// "All .ino files in the sketch folder (shown in the IDE as tabs with no extension) are concatenated together,
// starting with the file that matches the folder name followed by the others in alphabetical order, and the .cpp
// extension is added to the filename."