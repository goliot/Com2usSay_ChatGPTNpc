public class RolePlaying
{
    public string Profile { get; private set; }
    public string Advanced { get; private set; }
    public string Greeting { get; private set; }
    public string GenreAndTarget { get; private set; }
    public string Format { get; private set; }

    public RolePlaying(string profile, string advanced, string greeting, string genreAndTarget, string format)
    {
        Profile = profile;
        Advanced = advanced;
        Greeting = greeting;
        GenreAndTarget = genreAndTarget;
        Format = format;
    }
}
