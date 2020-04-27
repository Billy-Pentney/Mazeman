using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.IO;

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
            PositionTxtBlock.Foreground = GameConstants.ForegroundColour;
            PositionTxtBlock.Background = GameConstants.BackgroundColour;
            NameTxtBlock.Foreground = GameConstants.ForegroundColour;
            NameTxtBlock.Background = GameConstants.BackgroundColour;
            ScoreTxtBlock.Foreground = GameConstants.ForegroundColour;
            ScoreTxtBlock.Background = GameConstants.BackgroundColour;

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
            }

        }
    }
}
