using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using System.Collections.Generic;
using System;

namespace Assets
{
    public class MysteryWord
    {
        public string eWord;
        public string cWord;
        public string description;
        public int level;
        public bool isChineseHint;
        public List<int> hintPos = new List<int>();
        
        public void SetWord(Dictionary<string, object> data)
        {
            eWord = data["english"].ToString().ToLower();
            cWord = data["chinese"].ToString();
            description = data["hint"].ToString();
            int length = eWord.Length;
            isChineseHint = false;
            int hintLetterCount = 0;
            if(length < 6)
            {
                level = 1;
                isChineseHint = false;
                hintLetterCount = 1;
            }
            else if(length < 9)
            {
                level = 2;
                isChineseHint = true;
                hintLetterCount = 3;
            }
            else if(length < 11)
            {
                level = 3;
                isChineseHint = true;
                hintLetterCount = 4;
            }
            else
            {
                level = 4;
                isChineseHint = false;
                hintLetterCount = 5;
            }
            hintPos = GenerateUniqueRandomNumbers(eWord.Length - 1, hintLetterCount);
        }
        
        List<int> GenerateUniqueRandomNumbers(int n, int count)
        {
            List<int> numbers = new List<int>();
            System.Random random = new System.Random();

            // Fill the list with numbers from 0 to n
            for (int i = 0; i <= n; i++)
            {
                numbers.Add(i);
            }

            List<int> result = new List<int>();

            for (int i = 0; i < count; i++)
            {
                int randomIndex =  random.Next(n - i + 1);
                result.Add(numbers[randomIndex]);

                // Swap the chosen number with the last number in the list
                numbers[randomIndex] = numbers[n - i];
            }

            return result;
        }
    }
    
    public class Common : MonoBehaviour
    {
        public static string GenerateUniqueID(int min=11, int max=17)
        {
            int length = UnityEngine.Random.Range(min, max);
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            string unique_id = "";
            for (int i = 0; i < length; i++)
            {
                unique_id += chars[UnityEngine.Random.Range(0, chars.Length - 1)];
            }
            return unique_id;
        }
        
        static public void DestroyChildren(GameObject obj)
        {
            DestroyChildren(obj.transform);
        }
        static public void DestroyChildren(Component comp)
        {
            DestroyChildren(comp.transform);
        }
        static public void DestroyChildren(Transform t)
        {
            for (int i = t.childCount - 1; i >= 0; i--)
            {
                var child = t.GetChild(i);
                GameObject.Destroy(child.gameObject);
            }
            t.DetachChildren();
        }

        static public char GetAlphaFromIndex(int index)
        {
            return (char)((int)'a' + index - 1);
        }

        static public int GetScoreFromString(string word)
        {
            int score = 0;
            foreach(char c in word)
            {
                switch (c - 'a' + 'A')
                {
                    case (char)'A':
                    case (char)'E':
                    case (char)'I':
                    case (char)'O':
                    case (char)'U':
                        score ++;
                        break;
                    case (char)'D':
                    case (char)'G':
                    case (char)'L':
                    case (char)'N':
                    case (char)'S':
                    case (char)'T':
                    case (char)'R':
                        score += 2;
                        break;
                    case (char)'B':
                    case (char)'C':
                    case (char)'M':
                    case (char)'P':
                        score += 3;
                        break;
                    case (char)'F':
                    case (char)'H':
                    case (char)'V':
                    case (char)'W':
                    case (char)'Y':
                        score += 4;
                        break;
                    case (char)'K':
                        score += 5;
                        break;
                    case (char)'J':
                    case (char)'X':
                        score += 8;
                        break;
                    case (char)'Q':
                    case (char)'Z':
                        score += 10;
                        break;
                }
            }
            return score;
        }

        static public int GetScoreFromIndex(int index)
        {
            var c = GetAlphaFromIndex(index);
            c = (char)(c - 'a' + 'A');
            switch (c)
            {
                case (char)'A':
                case (char)'E':
                case (char)'I':
                case (char)'O':
                case (char)'U':
                    return 1;
                case (char)'D':
                case (char)'G':
                case (char)'L':
                case (char)'N':
                case (char)'S':
                case (char)'T':
                case (char)'R':
                    return 2;
                case (char)'B':
                case (char)'C':
                case (char)'M':
                case (char)'P':
                    return 3;
                case (char)'F':
                case (char)'H':
                case (char)'V':
                case (char)'W':
                case (char)'Y':
                    return 4;
                case (char)'K':
                    return 5;
                case (char)'J':
                case (char)'X':
                    return 8;
                case (char)'Q':
                case (char)'Z':
                    return 10;
            }
            return 0;
        }
    }
}