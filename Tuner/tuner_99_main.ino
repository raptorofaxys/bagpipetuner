void setup()                                        // run once, when the sketch starts
{
    Serial.begin(115200); // for debugging
    pinMode(kStatusPin, OUTPUT);            // sets the digital pin as output
    pinMode(kStatusPin2, OUTPUT);            // sets the digital pin as output
    digitalWrite(kStatusPin, LOW);
    digitalWrite(kStatusPin2, LOW);
}

void loop()
{
    Tuner tuner;
    tuner.Go();
}
