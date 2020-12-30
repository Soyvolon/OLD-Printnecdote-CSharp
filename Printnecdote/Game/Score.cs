
namespace Printnecdote
{
    public struct Score
    {
        public int DamageDelt { get; private set; }
        public int Faints { get; private set; }
        public int DamageRecived { get; private set; }
        public int StartingScore { get; set; }

        /// <summary>
        /// Creates a new score object with a starting score (defaults to 0)
        /// </summary>
        /// <param name="score">The starting score. Deafults to 0</param>
        public Score(int score = 0)
        {
            DamageDelt = 0;
            Faints = 0;
            DamageRecived = 0;
            StartingScore = score;
        }

        /// <summary>
        /// Updates the Damage Delt value
        /// </summary>
        /// <param name="dmg">Damage delt out</param>
        public void UpdateDamageDelt(int dmg)
        {
            DamageDelt += dmg;
        }

        public void AddFaint()
        {
            Faints++;
        }

        public void UpdateDamageRecived(int dmg)
        {
            DamageRecived += dmg;
        }

        /// <summary>
        /// Gets the TotalScore with the new score.
        /// </summary>
        public int GetTotalScore()
        {
            // Each damage delt is word 2 points.
            // Each faint is worth -50 points.
            // Each damage recived is work -1 point.

            int score = StartingScore;

            for(int i = 0; i < DamageDelt; i++)
            {
                score += 2;
            }
            for(int i = 0; i < Faints; i++)
            {
                score -= 50;
            }
            for(int i = 0; i < DamageRecived; i++)
            {
                score -= 1;
            }

            return score;
        }
    }
}
