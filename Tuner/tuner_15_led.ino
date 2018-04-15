void Blink(int times = 1)
{
    for (int i = 0; i < times; ++i)
    {
        digitalWrite(kStatusPin, HIGH);
        delay(kDebounceMs);
        digitalWrite(kStatusPin, LOW);
        delay(kDebounceMs);
    }
}
