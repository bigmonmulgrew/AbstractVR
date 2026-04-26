using UnityEngine;
using Oculus.Voice.Dictation;
public class ShootToRecord : ShootUI
{
    AppDictationExperience dictationExperience;

    private void Awake()
    {
        dictationExperience = GetComponentInChildren<AppDictationExperience>();
    }
    public override void Hit()
    {
        if (isActive) return;
        dictationExperience.Activate();
        
    }

}
