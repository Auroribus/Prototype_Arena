using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace _Scripts.Refactor.SFX
{
    public class SFXController : MonoBehaviour {

        public static SFXController instance;

        public List<SFXClip> SFX_List = new List<SFXClip>();

        private AudioSource audio_source;

        private void Awake()
        {
            if (instance == null)
                instance = this;

            audio_source = GetComponent<AudioSource>();
        }

        public void PlaySFXClip(string sfx_name)
        {
            int clip_index = 0;
            //get clip index
            foreach(SFXClip clip in SFX_List)
            {
                if(clip.sfx_name == sfx_name)
                {
                    clip_index = SFX_List.IndexOf(clip);
                    //Debug.Log("Clip: " + clip_index);
                    break;
                }
            }

        

            //set volume and pitch
            audio_source.volume = SFX_List[clip_index].sfx_volume;
            audio_source.pitch = SFX_List[clip_index].sfx_pitch;

            //if theres multiple possible clips, choose one randomly
            int random_sfx_clip_index = Random.Range(0, SFX_List[clip_index].sfx_clips.Count);
            audio_source.PlayOneShot(SFX_List[clip_index].sfx_clips[0]);        
        }
    }

    [System.Serializable]
    public class SFXClip
    {
        public string sfx_name;

        public List<AudioClip> sfx_clips = new List<AudioClip>();

        [Range(0.0f, 1.0f)]
        public float sfx_volume;

        [Range(-3.0f, 3.0f)]
        public float sfx_pitch;
    }
}