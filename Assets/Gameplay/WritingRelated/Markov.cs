using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;

[System.Serializable]
public class Markov
{
    static readonly List<char> ALPHABET = new List<char>(
        "abcedfghijklmnopqrstuvwxyz,.!?;:-\'\" ");
    static readonly List<char> END_OF_LINE = new List<char>(".....!?");
    Dictionary<string, int[]> table;
    string[] corpus_words;
    List<string> initial_states;
    int size;

    public Markov(string corpus, int size)
    {
        this.size = size;
        corpus = corpus.ToLower();
        System.Text.StringBuilder temp = new System.Text.StringBuilder(corpus.Length);
        for(int i = 0; i < corpus.Length; i++)
        {
            if (ALPHABET.Contains(corpus[i]))
                temp.Append(corpus[i]);
            else if (corpus[i].Equals('\n'))
                temp.Append(' ');
        }
        corpus = temp.ToString();

        corpus_words = corpus.Split(' ');
        initial_states = new List<string>(100);

        string arr = "";
        for (int i = 0; i < corpus_words.Length; i++)
        {
            arr += (i == 0 ? "" : ", ")
                + "'" + corpus_words[i] + "'";
            if (corpus_words[i].Length >= size)
            {
                initial_states.Add(corpus_words[i]);
            }
        }
        Debug.Log(arr);

        //generate table based on corpus
        table = new Dictionary<string, int[]>();

        for (int i = 0; i < corpus.Length - size - 1; i++)
        {
            string state = corpus.Substring(i, size);
            char next = corpus.Substring(i + size, 1).ToCharArray()[0];
            //Debug.Log("state: " + state + "\t next: " + next + " = " + (int)next);

            if (table.ContainsKey(state))
            {
                int[] row = table[state];
                row[ALPHABET.IndexOf(next)]++;
            }
            else
            {
                int[] row = new int[ALPHABET.Count];
                for(int j = 0; j < ALPHABET.Count; j++)
                {
                    row[j] = 0;
                }
                row[ALPHABET.IndexOf(next)] = 1;
                table.Add(state, row);
            }
        }
    }

    //generate sentence of up to length words. the sentence will terminate upon certain puncuations
    public string Run(int length)
    {
        //pick first word from corpus
        string initial_state = initial_states[Mathf.FloorToInt(Random.value * initial_states.Count)]+' ';
        string window = initial_state.Substring(0, size);

        //generate up to length words
        int reprompt = 25;
        System.Tuple<string, float> best_sentence = new System.Tuple<string, float>("", -100);
        for (int sentence = 0; sentence < reprompt; sentence++)
        {
            bool early_termination = false;
            System.Text.StringBuilder result = new System.Text.StringBuilder(length * 10);
            for (int i = 0; i < length && !early_termination; i++)
            {
                //generate new word
                char next;
                do
                {
                    next = NextLetter(window);
                    if (next != '\0')
                    {
                        result.Append(next);
                        window = window.Substring(1) + next;
                    }
                    //terminate sentence if certain symbols are reached
                    if (END_OF_LINE.Contains(next) || next == '\0')
                    {
                        //Debug.Log("terminating line early: " + next);
                        early_termination = true;
                        break;
                    }
                } while (next != ' ' && result.Length < length * 20);
            }
            //if this is reached, then the sentence has not terminated by puncutation
            //punctuation will be manually added
            result.Remove(result.Length - 1, 1);
            result.Append(END_OF_LINE[Mathf.FloorToInt(Random.value * END_OF_LINE.Count)]);

            string r = result.ToString();
            r = Regex.Replace(r, @"\s+", " ");

            System.Tuple<string, float> scored = ScoreSentence(r);
            if (scored.Item2 > best_sentence.Item2)
            {
                best_sentence = scored;
            }
        }

        return best_sentence.Item1;
    }

    private System.Tuple<string, float> ScoreSentence(string s)
    {
        //simple evaluation of sentence score based on its length
        int penalties = 0;

        /*
         * deprecated: parenthesis are no longer generated due to confusing rules
        //penalties for unpaired symbols
        System.Tuple<char, char>[] pairs = new System.Tuple<char, char>[]
        {
            new System.Tuple<char, char>('(', ')'),
            new System.Tuple<char, char>('\"', '\"')
        };

        //in each pair, the left element counts as a +1, and the right element counts as -1
        //if both occurred, then the net sum = 0
        //if not, then the net sum is some nonzero number
        int[] pair_count = new int[pairs.Length];

        char[] c_arr = s.ToCharArray();
        foreach (char c in c_arr)
        {
            for (int i = 0; i < pairs.Length; i++)
            {
                if (c == pairs[i].Item1) { pair_count[i]++; }
                else if (c == pairs[i].Item2) { pair_count[i]--; }
            }
        }

        for (int i = 0; i < pairs.Length; i++)
        {
            //for duplicate pairs like "" where item1 = item2
            //the number of lone pairs is 1 if item1 occurred an odd number of times
            //0 if even number of times
            if (pairs[i].Item1 == pairs[i].Item2)
            {
                pair_count[i] = pair_count[i] % 2;
            }
            penalties += 10 * pair_count[i];
        }

        //append left/right members of each pair until everything cancels out
        for (int i = 0; i < pairs.Length; i++)
        {
            while (pair_count[i] < 0)
            {
                s = s + pairs[i].Item1;
                pair_count[i]++;
            }
            while (pair_count[i] > 0)
            {
                s = s + pairs[i].Item2;
                pair_count[i]--;
            }
        }*/

        char[] c_arr_padded = (' ' + s + ' ').ToCharArray();
        Regex lone_letter_rightside = new Regex(@"[,.!?;:()\- ]");
        for (int i = 0; i < c_arr_padded.Length-2; i++)
        {
            if (c_arr_padded[i] == ' '
                && c_arr_padded[i + 1] != 'a'
                && c_arr_padded[i + 1] != 'i'
                && lone_letter_rightside.Match(c_arr_padded[i + 2].ToString()).Success
                )
            {
                //Debug.Log("\t loner spotted: " + c_arr_padded[i] + c_arr_padded[i + 1] + c_arr_padded[i + 2]);
                penalties += 10;
            }
        }

        // consecutive punctuation
        penalties += Regex.Matches(s, @"[,.!?;:][,.!?;:]").Count * 15;

        // capitalize first letter
        for (int i = 0; i < s.Length; i++)
        {
            if (char.IsLetter(s[i]))
            {
                s = s.Substring(0, i) + char.ToUpper(s[i])
                    + s.Substring(i + 1);
                break;
            }
        }
        s = s.Replace(" i ", " I ");

        return new System.Tuple<string, float>(s, s.Length - penalties);
    }

    private char NextLetter(string state)
    {
        if (table.ContainsKey(state))
        {
            int[] row = table[state];

            int[] tally = new int[ALPHABET.Count];
            int sum = 0;
            for(int i = 0; i < ALPHABET.Count; i++)
            {
                tally[i] = row[i] + (i > 0 ? tally[i - 1] : 0);
                sum += row[i];
            }
            float index = Random.value * sum;
            for (int i = 0; i < ALPHABET.Count; i++)
            {
                if (index <= tally[i]) { return ALPHABET[i]; }
            }
        }
        //key is not found

        Debug.LogError(state + " has no corresponding next state!");
        return '\0';
    }
}
