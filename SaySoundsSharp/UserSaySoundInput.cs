namespace SaySoundsSharp;

public class UserSaySoundInput {

    public readonly string soundName;
    public readonly float volume;
    public readonly float pitch;

    public UserSaySoundInput(string soundName, float volume = 1.0F, float pitch = 1.0F) {
        this.soundName = soundName;

        if(volume > 1.0F) {
            this.volume = 1.0F;
        }
        else if(volume < 0.0F) {
            this.volume = 0.0F;
        }
        else {
            this.volume = volume;
        }

        if(pitch > 1.0F) {
            this.pitch = 1.0F;
        }
        else if(pitch < 0.0F) {
            this.pitch = 0.0F;
        }
        else {
            this.pitch = pitch;
        }

    }
}