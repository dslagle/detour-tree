using RouteMatch.CA.DayOfDetour.Model;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DetourList
{
    public partial class Form1 : Form
    {
        DetourMasterRoute mr1 = new DetourMasterRoute() { Name = "Route 1" };
        DetourMasterRoute mr2 = new DetourMasterRoute() { Name = "Route 2" };

        DetourSubroute sr1 = new DetourSubroute() { Name = "Pattern 1" };
        DetourSubroute sr2 = new DetourSubroute() { Name = "Pattern 2" };
        DetourSubroute sr3 = new DetourSubroute() { Name = "Pattern 3" };
        DetourSubroute sr4 = new DetourSubroute() { Name = "Pattern 4" };

        DetourStop ds1 = new DetourStop() { Name = "Stop 1" };
        DetourStop ds2 = new DetourStop() { Name = "Stop 2", Canceled = true };
        DetourStop ds3 = new DetourStop() { Name = "Stop 3" };
        DetourStop ds4 = new DetourStop() { Name = "Stop 4" };
        DetourStop ds5 = new DetourStop() { Name = "Stop 1" };
        DetourStop ds6 = new DetourStop() { Name = "Stop 2" };
        DetourStop ds7 = new DetourStop() { Name = "Stop 3" };
        DetourStop ds8 = new DetourStop() { Name = "Stop 4" };

        Detour detour = new Detour() { Name = "Snow Storm" };

        public Form1()
        {
            InitializeComponent();

            sr1.Stops.Add(ds1);
            sr1.Stops.Add(ds2);
            sr2.Stops.Add(ds3);
            sr2.Stops.Add(ds4);
            sr3.Stops.Add(ds5);
            sr3.Stops.Add(ds6);
            sr4.Stops.Add(ds7);
            sr4.Stops.Add(ds8);

            mr1.Subroutes.Add(sr1);
            mr1.Subroutes.Add(sr2);
            mr2.Subroutes.Add(sr3);
            mr2.Subroutes.Add(sr4);

            detour.MasterRoutes.Add(mr1);
            detour.MasterRoutes.Add(mr2);

            detourSelectionList1.RenderModel(detour);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            ds1.Canceled = !ds1.Canceled;
        }
    }
}
