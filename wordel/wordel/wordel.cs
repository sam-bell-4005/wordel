using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace wordel
{
    public partial class wordel : Form
    {
        int mouse_x, mouse_y, keyboard_x = 60, keyboard_y = 400, key_size = 40;
        int guess, letter, word_count, game_state, error_state;

        string[,] keyboard = { 
            { "Q", "W", "E", "R", "T", "Y", "U", "I", "O", "P" },
            { "A", "S", "D", "F", "G", "H", "J", "K", "L", "" },
            { "ENTER", "Z", "X", "C", "V", "B", "N", "M", "BKSP", "" }
        };

        string[] word_list = new string[5000];
        string[,] guesses = new string[6, 5];
        string answer;

        int[] graph = new int[8];
        int[,] keyboard_state = new int[3, 10];
        int[,] guess_state = new int[6,5];

        Brush[] palette = { Brushes.LightGray
                , Brushes.DarkGray
                , Brushes.MediumSeaGreen
                , Brushes.Goldenrod };

        Font font_huge = new Font("Arial", 24, FontStyle.Bold);
        Font font_big = new Font("Arial", 12, FontStyle.Bold);
        Font font_small = new Font("Arial", 8);

        public wordel()
        {
            LoadDictionary();
            NewGame();
            keyboard_state[1, 9] = 1;
            keyboard_state[2, 9] = 1;
            InitializeComponent();
        }

        private void NewGame()
        {
            Random ra = new Random();  
            guess = 0;
            letter = 0;
            answer = word_list[ra.Next(word_count)];
            for (int x = 0; x < 10; x++)
                for (int y = 0; y < 3; y++)
                    keyboard_state[y, x] = 0;
            for (int x = 0; x < 5; x++)
                for (int y = 0; y < 6; y++)
                {
                    guesses[y, x] = "";
                    guess_state[y, x] = 0;
                }
            game_state = 0;
            error_state = -1;
        }

        private void pictureBox1_MouseUp(object sender, MouseEventArgs e)
        {
            int bx, by;
            bx = (mouse_x - keyboard_x) / key_size;
            by = (mouse_y - keyboard_y) / key_size;
            if (game_state > 0)
                NewGame();
            else
            {
                if (bx >= 0 && bx <= 9 && by >= 0 && by < 3)
                {
                    if (keyboard[by, bx] == "BKSP")
                    {
                        if (letter > 0)
                        {
                            error_state = -1;
                            letter--;
                            guesses[guess, letter] = "";
                        }
                    }
                    else if (keyboard[by, bx] == "ENTER")
                    {
                        if (letter == 5)
                            EvaluateGuess();
                    }
                    else
                    {
                        if (letter < 5)
                        {
                            guesses[guess, letter] = keyboard[by, bx];
                            letter++;
                        }
                    }
                }
            }
            pictureBox1.Invalidate();
        }

        private void EvaluateGuess()
        {
            string word = guesses[guess, 0] + guesses[guess, 1] + guesses[guess, 2] + guesses[guess, 3] + guesses[guess, 4];
            bool is_word = false;
            for (int w = 0; w < word_count; w++)
                if (word_list[w] == word)
                    is_word = true;
            if (is_word)
            {
                bool[] mark = new bool[5];
                int ct = 0;
                for (int l = 0; l < 5; l++)
                    if (guesses[guess, l] == answer.Substring(l, 1))
                    {
                        guess_state[guess, l] = 2;
                        ct++;
                        mark[l] = true;
                    }
                if (ct < 5)
                {
                    for (int l = 0; l < 5; l++)
                    {
                        bool f = false;
                        for (int a = 0; a < 5; a++)
                            if (!f && !mark[a] && a != l && guess_state[guess, l] == 0 && guesses[guess, l] == answer.Substring(a, 1))
                            {
                                f = true;
                                guess_state[guess, l] = 3;
                                mark[a] = true;
                            }
                    }

                    for (int l = 0; l < 5; l++)
                    {
                        if (guess_state[guess, l] == 0)
                            guess_state[guess, l] = 1;
                    }

                }
                else
                {
                    game_state = 1;
                    graph[guess + 1]++;
                }

                guess++;
                MarkKeyboard();

                letter = 0;
                if (guess > 5 && game_state == 0)
                {
                    guess = 7;
                    graph[7]++;
                    game_state = 2;
                }
            }
            else
                error_state = guess;
        }

        private void MarkKeyboard()
        {
            for (int g = 0; g < guess; g++)
                for (int l = 0; l < 5; l++)
                    if (guess_state[g, l] == 1)
                        for (int x = 0; x < 10; x++)
                            for (int y = 0; y < 3; y++)
                                if (guesses[g, l] == keyboard[y, x])
                                    keyboard_state[y, x] = 1;
            for (int g = 0; g < guess; g++)
                for (int l = 0; l < 5; l++)
                    if (guess_state[g, l] == 3)
                        for (int x = 0; x < 10; x++)
                            for (int y = 0; y < 3; y++)
                                if (guesses[g, l] == keyboard[y, x])
                                    keyboard_state[y, x] = 3;
            for (int g = 0; g < guess; g++)
                for (int l = 0; l < 5; l++)
                    if (guess_state[g, l] == 2)
                        for (int x = 0; x < 10; x++)
                            for (int y = 0; y < 3; y++)
                                if (guesses[g, l] == keyboard[y, x])
                                    keyboard_state[y, x] = 2;
        }

        private void LoadDictionary()
        {
            StreamReader sr = new StreamReader("dict5.txt");
            word_count = 0;
            while (!sr.EndOfStream)
            {
                word_list[word_count] = sr.ReadLine().ToUpper();
                word_count++;
            }
            sr.Close();
        }

        private void pictureBox1_Paint(object sender, PaintEventArgs e)
        {
            e.Graphics.Clear(Color.White);
            DrawKeyboard(e);

            DrawGuesses(e);
            if (game_state > 0)
            {
                DrawGraph(e);

            }
        }

        private void DrawKeyboard(PaintEventArgs e)
        {
            for (int x = 0; x < 10; x++)
                for (int y = 0; y < 3; y++)
                {
                    e.Graphics.FillRectangle(palette[keyboard_state[y, x]], keyboard_x + key_size * x, keyboard_y + key_size * y, key_size, key_size);
                    e.Graphics.DrawRectangle(Pens.Gray, keyboard_x + key_size * x, keyboard_y + key_size * y, key_size, key_size);
                    e.Graphics.DrawString(keyboard[y, x], keyboard[y, x].Length > 1 ? font_small : font_big, Brushes.Black, keyboard_x + key_size * x, keyboard_y + key_size * y);

                }
        }

        private void DrawGuesses(PaintEventArgs e)
        {
            int gx = 40, gy = 20, gs = 55, gsp = 60;
            for (int y = 0; y < 6; y++)
                for (int x = 0; x < 5; x++)
                {

                    e.Graphics.FillRectangle(palette[guess_state[y, x]], gx + x * gsp, gy + y * gsp, gs, gs);
                    e.Graphics.DrawRectangle(Pens.Gray, gx + x * gsp, gy + y * gsp, gs, gs);
                    e.Graphics.DrawString(guesses[y, x], font_huge, Brushes.Black, gx + x * gsp, gy + y * gsp);
                }
            if (letter < 5 && guess < 6)
            {
                e.Graphics.DrawRectangle(new Pen(Color.Gold, 3), gx + letter * gsp, gy + guess * gsp, gs, gs);
            }
            if (error_state >= 0)
                e.Graphics.DrawString("THAT'S NOT A WORD", font_big, Brushes.Red, gx + 6 * gsp, gy + error_state * gsp);
            if (game_state == 1)
                e.Graphics.DrawString("YOU GOT IT", font_big, Brushes.Green, gx + 6 * gsp, gy);
            if (game_state == 2)
            {
                e.Graphics.DrawString("FAIL", font_big, Brushes.Red, gx + 6 * gsp, gy);
                e.Graphics.DrawString(answer, font_big, Brushes.Green, gx + 7 * gsp, gy);
            }
        }

        private void DrawGraph(PaintEventArgs e)
        {
            int m = 1;
            for (int g = 0; g < 8; g++)
                if (graph[g] > m)
                    m = graph[g];

            for (int g = 1; g < 8; g++)
            {
                e.Graphics.DrawString(g == 7 ? "F" : g.ToString(), font_big, Brushes.Black, 400, 100 + g * 25);
                e.Graphics.FillRectangle(g == guess ? (guess == 7?Brushes.Red:Brushes.MediumSeaGreen) : Brushes.Gray, 420, 100 + g * 25, graph[g] * 250 / m, 20);
                e.Graphics.DrawRectangle(Pens.DarkGray, 420, 100 + g * 25, graph[g] * 250 / m, 20);
                if (graph[g] > 0)
                    e.Graphics.DrawString(graph[g].ToString(), font_small, Brushes.White, 422, 102 + g * 25);

            }
        }

        private void pictureBox1_MouseMove(object sender, MouseEventArgs e)
        {
            mouse_x = e.X;
            mouse_y = e.Y;
        }
    }
}
