using UnityEngine;

namespace DialogueScripts
{
    [System.Serializable]
    public class Dialogue {

        public string name;
        
        public string[] sentences;
        
        public string[] hackedSentences;
        public string[] purpleSentences;

        public bool hacked;

    }
}