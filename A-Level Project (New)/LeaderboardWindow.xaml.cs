using System;
using System.Collections.Generic;
using System.Windows;

namespace A_Level_Project__New_
{
    /// <summary>
    /// Interaction logic for LeaderboardWindow.xaml
    /// </summary>

    public partial class LeaderboardWindow : Window
    {
        public LeaderboardWindow(List<DataEntry> entries)
        {
            List<DataEntry> TopSinglePlayerEntries = new List<DataEntry>();

            InitializeComponent();
            TopSinglePlayerEntries = entries;

            LeaderboardCanvas.Background = GameConstants.BackgroundColour;
            PositionLbl.Foreground = GameConstants.ForegroundColour;
            NameLbl.Foreground = GameConstants.ForegroundColour;
            ScoreLbl.Foreground = GameConstants.ForegroundColour;
            TimeLbl.Foreground = GameConstants.ForegroundColour;
            PositionTxtBlock.Foreground = GameConstants.ForegroundColour;
            PositionTxtBlock.Background = GameConstants.BackgroundColour;
            NameTxtBlock.Foreground = GameConstants.ForegroundColour;
            NameTxtBlock.Background = GameConstants.BackgroundColour;
            ScoreTxtBlock.Foreground = GameConstants.ForegroundColour;
            ScoreTxtBlock.Background = GameConstants.BackgroundColour;
            TimeTxtBlock.Foreground = GameConstants.ForegroundColour;
            TimeTxtBlock.Background = GameConstants.BackgroundColour;

            UpdateTextblock(TopSinglePlayerEntries);
        }

        private void UpdateTextblock(List<DataEntry> EntriesToDisplay)
        {
            int count = 0;
            string gap = Environment.NewLine + Environment.NewLine;

            if (EntriesToDisplay.Count >= 10)
            {
                count = 10;
            }
            else
            {
                count = EntriesToDisplay.Count;
            }

            for (int i = 0; i < count; i++)
            {
                PositionTxtBlock.Inlines.Add(Convert.ToString(i+1) + gap);
                NameTxtBlock.Inlines.Add(EntriesToDisplay[i].GetNameofHighestScore() + gap);
                ScoreTxtBlock.Inlines.Add(Convert.ToString(EntriesToDisplay[i].GetHighestScore()) + gap);
                TimeTxtBlock.Inlines.Add(Convert.ToString(EntriesToDisplay[i].SurvivedFor) + gap);
            }

        }
    }
}
