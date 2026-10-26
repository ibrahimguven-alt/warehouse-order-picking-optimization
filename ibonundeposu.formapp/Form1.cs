using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace ibonundeposu.formapp
{
    public partial class Form1 : Form
    {
        // ==========================================
        // GLOBAL DEÄÄ°ÅKENLER
        // ==========================================
        int blocks = 0;
        int aisles = 0;
        int locations = 0;
        int depot = 1;
        int shelfCount = 1;
        int selectedShelf = 1;
        int optionalAisleLength = 0;
        int optionalCrossoverLength = 0;
        decimal warehouseHeight = 0;

        // Boyut sabitleri
        private const int CellW = 18;
        private const int CellH = 18;
        private const int CorridorW = 30;
        private const int CrossAisleH = 30;
        private const int StartX = 60;
        private const int StartY = 50;

        private Rectangle warehouseBounds;
        private RectangleF depotRect;

        private bool layoutHazir = false;

        // Ã‡izim sistemi
        private bool cizimModu = false;
        private Point sonMouseNoktasi;

        Random rnd = new Random();

        // ==========================================
        // MODEL LÄ°STELERÄ°
        // ==========================================
        private List<RackCell> hucreler = new List<RackCell>();
        private List<CrossoverPoint> crossoverNoktalari = new List<CrossoverPoint>();
        private List<Point> rotaYolu = new List<Point>();

        private readonly Dictionary<string, double> routeResults = new Dictionary<string, double>();
        private string activeRouteName = null;
        private int manualRouteNumber = 1;
        private readonly Dictionary<string, int> stableSegmentOffsets = new Dictionary<string, int>();
        private readonly string loggedInUser;
        private readonly string loggedInRole;
        private ComboBox cmbLanguage;
        private Label lblLanguage;
        private Button btnExit;
        private Label lblShelfSelector;
        private Label lblCurrentShelf;
        private ComboBox cmbShelfSelector;
        private Label lblVehicle;
        private ComboBox cmbVehicle;
        private string currentLanguage = "en";
        private readonly Dictionary<string, Dictionary<string, string>> translations =
            new Dictionary<string, Dictionary<string, string>>();

        // ==========================================
        // HÃœCRE MODELÄ°
        // ==========================================
        private class RackCell
        {
            public Rectangle Bounds { get; set; }
            public int Index { get; set; }

            public bool IsSelected { get; set; } = false;
            public bool IsPicked { get; set; } = false;
            public HashSet<int> SelectedShelves { get; set; } = new HashSet<int>();
            public HashSet<int> PickedShelves { get; set; } = new HashSet<int>();
        }

        // ==========================================
        // CROSSOVER MODELÄ°
        // ==========================================
        private class CrossoverPoint
        {
            public Point Point { get; set; }
            public bool IsVisited { get; set; } = false;
        }

        private class RouteNode
        {
            public Point Point { get; set; }
            public bool IsDepot { get; set; }
            public RackCell Cell { get; set; }
        }

        private class RouteDrawSegment
        {
            public Point From { get; set; }
            public Point To { get; set; }
            public string Key { get; set; }
        }

        private class RatliffRosenthalAisle
        {
            public int X { get; set; }
            public bool HasItems { get; set; }
            public int TopMostItemY { get; set; }
            public int BottomMostItemY { get; set; }
            public int TopLoopEndY { get; set; }
            public int BottomLoopEndY { get; set; }
        }

        private class RatliffRosenthalCandidate
        {
            public double Cost { get; set; }
            public List<Point> Route { get; set; }
        }

        // ==========================================
        // CONSTRUCTOR
        // ==========================================
        public Form1() : this("unknown")
        {
        }

        public Form1(string username)
        {
            loggedInUser = string.IsNullOrWhiteSpace(username) ? "unknown" : username.Trim();
            loggedInRole = AuthService.GetRole(loggedInUser);
            InitializeComponent();

            radioButton1.Checked = true;
            InitializeTranslations();
            OverrideCleanTranslations();
            ApplyInteractiveWarehouseStyle();
            InitializeTopControls();
            InitializeWarehouseLayerControls();
            ApplyLanguage(AppText.CurrentLanguage);
            SetDefaultInputs();
            ApplyRolePermissions();

            panelWarehouse.Paint += panelWarehouse_Paint;
            panelWarehouse.MouseClick += panelWarehouse_MouseClick;
            panelWarehouse.MouseMove += panelWarehouse_MouseMove;

            button4.Click += (s, e) => StartManualRoute();
            button5.Click += (s, e) => RunOptimalRoute();
            button8.Click += (s, e) => RunSShapeRoute("Largest Gap");
            button9.Click += (s, e) => RunSShapeRoute("Aisle-by-aisle");
            button10.Click += (s, e) => RunCombinedRoute("Combined +");
        }

        private void ApplyInteractiveWarehouseStyle()
        {
            Text = "Interactive Warehouse - Depo Rota Planlama";
            BackColor = Color.FromArgb(240, 243, 247);
            Font = new Font("Segoe UI", 9F);
            ClientSize = new Size(980, 610);

            rbLayout.BackColor = Color.FromArgb(31, 45, 61);
            panel1.BackColor = Color.White;
            panelWarehouse.Location = new Point(270, 46);
            panelWarehouse.Size = new Size(680, 500);
            panelWarehouse.BackColor = Color.White;
            panelWarehouse.BorderStyle = BorderStyle.FixedSingle;

            foreach (var radio in new[] { radioButton1, radioButton2, radioButton3, radioButton4, radioButton5 })
            {
                radio.ForeColor = Color.White;
                radio.BackColor = Color.FromArgb(31, 45, 61);
                radio.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            }

            radioButton1.Text = "Set Layout";
            radioButton2.Text = "Create an Order";
            radioButton3.Text = "Create a Route";
            radioButton4.Text = "Routing Methods";
            radioButton5.Text = "Results";

            label1.Text = "SET LAYOUT";
            label2.Text = "Fill out this form, then click\nShow Warehouse.";
            label9.Text = "CREATE AN ORDER";
            label10.Text = "Enter the number of products\nto pick, then create a random\norder.";
            label11.Text = "You can also create an order\nmanually by clicking warehouse\nlocations. Click Clear Order to\nreset all selections.";
            label14.Text = "CREATE A ROUTE";
            label12.Text = "Click New Route, then start\nfrom depot and draw a route\nthrough the warehouse.\nSelected products turn black\nwhen the route reaches them.";
            label15.Text = "ROUTING METHODS";
            label13.Text = "Select a routing method to\ncalculate and display a picking\nroute for the current order.";
            label17.Text = "RESULTS";
            label16.Text = "route1\n\n\nroute2\n\n\nroute3\n\n\nyour best route\n\n\nOptimal\n\n\nS-shape\n\n\nCombined\n\n\nLargest Gap\n\n\nAisle-by-aisle\n\n\nCombined +";

            btnShowWarehouse.Text = "Show Warehouse";
            button2.Text = "Set Order";
            button3.Text = "Clear Order";
            button4.Text = "New Route";

            StyleButtons(this);
        }

        private void InitializeTopControls()
        {
            lblLanguage = new Label
            {
                AutoSize = true,
                Location = new Point(ClientSize.Width - 250, 8),
                Anchor = AnchorStyles.Top | AnchorStyles.Right,
                Font = new Font("Segoe UI", 8.5F, FontStyle.Bold),
                ForeColor = Color.FromArgb(31, 45, 61)
            };

            cmbLanguage = new ComboBox
            {
                DropDownStyle = ComboBoxStyle.DropDownList,
                Location = new Point(ClientSize.Width - 185, 5),
                Size = new Size(105, 24),
                Anchor = AnchorStyles.Top | AnchorStyles.Right
            };

            cmbLanguage.Items.AddRange(new object[]
            {
                "English",
                "\u4e2d\u6587",
                "Turkce",
                "Espanol",
                "\u10e5\u10d0\u10e0\u10d7\u10e3\u10da\u10d8"
            });
            cmbLanguage.SelectedIndex = 0;
            cmbLanguage.SelectedIndexChanged += (s, e) =>
            {
                string[] languages = { "en", "zh", "tr", "es", "ka" };
                AppText.CurrentLanguage = languages[cmbLanguage.SelectedIndex];
                ApplyLanguage(languages[cmbLanguage.SelectedIndex]);
            };

            string[] startupLanguages = { "en", "zh", "tr", "es", "ka" };
            int startupLanguageIndex = Array.IndexOf(startupLanguages, AppText.CurrentLanguage);
            if (startupLanguageIndex >= 0)
                cmbLanguage.SelectedIndex = startupLanguageIndex;

            btnExit = new Button
            {
                Location = new Point(ClientSize.Width - 72, 4),
                Size = new Size(62, 26),
                Anchor = AnchorStyles.Top | AnchorStyles.Right,
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(198, 40, 40),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 8.5F, FontStyle.Bold)
            };
            btnExit.FlatAppearance.BorderSize = 0;
            btnExit.Click += (s, e) => ConfirmExit();

            Controls.Add(lblLanguage);
            Controls.Add(cmbLanguage);
            Controls.Add(btnExit);
            lblLanguage.BringToFront();
            cmbLanguage.BringToFront();
            btnExit.BringToFront();
        }

        private void InitializeWarehouseLayerControls()
        {
            lblShelfSelector = new Label
            {
                AutoSize = true,
                Location = new Point(270, 560),
                Anchor = AnchorStyles.Left | AnchorStyles.Bottom,
                Font = new Font("Segoe UI", 8.5F, FontStyle.Bold)
            };

            cmbShelfSelector = new ComboBox
            {
                DropDownStyle = ComboBoxStyle.DropDownList,
                Location = new Point(360, 556),
                Size = new Size(90, 24),
                Anchor = AnchorStyles.Left | AnchorStyles.Bottom
            };
            cmbShelfSelector.SelectedIndexChanged += (s, e) =>
            {
                if (cmbShelfSelector.SelectedItem == null)
                    return;

                selectedShelf = (int)cmbShelfSelector.SelectedItem;
                UpdateCurrentShelfLabel();
                panelWarehouse.Invalidate();
            };

            lblCurrentShelf = new Label
            {
                AutoSize = true,
                Location = new Point(465, 560),
                Anchor = AnchorStyles.Left | AnchorStyles.Bottom,
                Font = new Font("Segoe UI", 8.5F, FontStyle.Bold),
                ForeColor = Color.FromArgb(31, 45, 61)
            };

            Controls.Add(lblShelfSelector);
            Controls.Add(cmbShelfSelector);
            Controls.Add(lblCurrentShelf);

            lblVehicle = new Label
            {
                AutoSize = true,
                Location = new Point(640, 560),
                Anchor = AnchorStyles.Left | AnchorStyles.Bottom,
                Font = new Font("Segoe UI", 8.5F, FontStyle.Bold)
            };

            cmbVehicle = new ComboBox
            {
                DropDownStyle = ComboBoxStyle.DropDownList,
                Location = new Point(710, 556),
                Size = new Size(120, 24),
                Anchor = AnchorStyles.Left | AnchorStyles.Bottom
            };
            cmbVehicle.Items.AddRange(new object[] { "Human", "Forklift", "Robot" });
            cmbVehicle.SelectedIndex = 0;

            Controls.Add(lblVehicle);
            Controls.Add(cmbVehicle);
            lblShelfSelector.BringToFront();
            cmbShelfSelector.BringToFront();
            lblCurrentShelf.BringToFront();
            lblVehicle.BringToFront();
            cmbVehicle.BringToFront();
        }

        private void PopulateShelfSelector()
        {
            if (cmbShelfSelector == null)
                return;

            cmbShelfSelector.Items.Clear();

            for (int i = 1; i <= shelfCount; i++)
                cmbShelfSelector.Items.Add(i);

            selectedShelf = Math.Min(Math.Max(selectedShelf, 1), shelfCount);
            cmbShelfSelector.SelectedItem = selectedShelf;
            UpdateCurrentShelfLabel();
        }

        private void UpdateCurrentShelfLabel()
        {
            if (lblCurrentShelf != null)
                lblCurrentShelf.Text = T("currentShelf") + ": " + selectedShelf + " / " + shelfCount;
        }

        private void ConfirmExit()
        {
            DialogResult result = MessageBox.Show(
                T("exitMessage"),
                T("exitTitle"),
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question
            );

            if (result == DialogResult.Yes)
                Close();
        }

        private void InitializeTranslations()
        {
            translations["en"] = new Dictionary<string, string>
            {
                ["title"] = "Interactive Warehouse - Route Planning",
                ["language"] = "Language",
                ["exit"] = "Exit",
                ["exitTitle"] = "Exit",
                ["exitMessage"] = "Are you sure you want to exit?",
                ["setLayout"] = "Set Layout",
                ["createOrder"] = "Create an Order",
                ["createRoute"] = "Create a Route",
                ["routingMethods"] = "Routing Methods",
                ["results"] = "Results",
                ["layoutHelp"] = "Fill out this form, then click\nShow Warehouse.",
                ["blocks"] = "number of blocks",
                ["aisles"] = "number of aisles",
                ["locations"] = "number of\nlocations per aisle",
                ["depotLocation"] = "depot location",
                ["aisleLength"] = "aisle length\n(optional)",
                ["crossoverLength"] = "crossover length\n(optional)",
                ["shelfCount"] = "number of shelves",
                ["warehouseHeight"] = "warehouse height",
                ["shelfSelector"] = "Shelf layer",
                ["currentShelf"] = "Current shelf",
                ["showWarehouse"] = "Show Warehouse",
                ["orderHelp"] = "Enter the number of products\nto pick, then create a random\norder.",
                ["manualOrderHelp"] = "You can also create an order\nmanually by clicking warehouse\nlocations. Click Clear Order to\nreset all selections.",
                ["setOrder"] = "Set Order",
                ["clearOrder"] = "Clear Order",
                ["routeHelp"] = "Click New Route, then start\nfrom depot and draw a route\nthrough the warehouse.\nSelected products turn black\nwhen the route reaches them.",
                ["newRoute"] = "New Route",
                ["methodsHelp"] = "Select a routing method to\ncalculate and display a picking\nroute for the current order.",
                ["resultLabels"] = "route1\n\n\nroute2\n\n\nroute3\n\n\nyour best route\n\n\nOptimal\n\n\nS-shape\n\n\nCombined\n\n\nLargest Gap\n\n\nAisle-by-aisle\n\n\nCombined +"
            };

            translations["zh"] = new Dictionary<string, string>(translations["en"])
            {
                ["title"] = "äº¤äº’å¼ä»“åº“ - è·¯çº¿è§„åˆ’",
                ["language"] = "è¯­è¨€",
                ["exit"] = "é€€å‡º",
                ["exitTitle"] = "é€€å‡º",
                ["exitMessage"] = "ç¡®å®šè¦é€€å‡ºå—ï¼Ÿ",
                ["setLayout"] = "è®¾ç½®å¸ƒå±€",
                ["createOrder"] = "åˆ›å»ºè®¢å•",
                ["createRoute"] = "åˆ›å»ºè·¯çº¿",
                ["routingMethods"] = "è·¯çº¿æ–¹æ³•",
                ["results"] = "ç»“æœ",
                ["layoutHelp"] = "å¡«å†™æ­¤è¡¨å•ï¼Œç„¶åç‚¹å‡»\næ˜¾ç¤ºä»“åº“ã€‚",
                ["blocks"] = "åŒºå—æ•°é‡",
                ["aisles"] = "é€šé“æ•°é‡",
                ["locations"] = "æ¯ä¸ªé€šé“çš„\nä½ç½®æ•°é‡",
                ["depotLocation"] = "ä»“åº“å£ä½ç½®",
                ["aisleLength"] = "é€šé“é•¿åº¦\n(å¯é€‰)",
                ["crossoverLength"] = "æ¨ªå‘é€šé“é•¿åº¦\n(å¯é€‰)",
                ["showWarehouse"] = "æ˜¾ç¤ºä»“åº“",
                ["orderHelp"] = "è¾“å…¥è¦æ‹£é€‰çš„äº§å“æ•°é‡ï¼Œ\nç„¶ååˆ›å»ºéšæœºè®¢å•ã€‚",
                ["manualOrderHelp"] = "ä¹Ÿå¯ä»¥ç‚¹å‡»ä»“åº“ä½ç½®\næ‰‹åŠ¨åˆ›å»ºè®¢å•ã€‚ç‚¹å‡»æ¸…é™¤è®¢å•\nå¯é‡ç½®æ‰€æœ‰é€‰æ‹©ã€‚",
                ["setOrder"] = "è®¾ç½®è®¢å•",
                ["clearOrder"] = "æ¸…é™¤è®¢å•",
                ["routeHelp"] = "ç‚¹å‡»æ–°è·¯çº¿ï¼Œç„¶åä» depot\nå¼€å§‹ç»˜åˆ¶ç©¿è¿‡ä»“åº“çš„è·¯çº¿ã€‚\nåˆ°è¾¾çš„äº§å“ä¼šå˜ä¸ºé»‘è‰²ã€‚",
                ["newRoute"] = "æ–°è·¯çº¿",
                ["methodsHelp"] = "é€‰æ‹©ä¸€ç§è·¯çº¿æ–¹æ³•ï¼Œ\nè®¡ç®—å¹¶æ˜¾ç¤ºå½“å‰è®¢å•çš„\næ‹£é€‰è·¯çº¿ã€‚",
                ["resultLabels"] = "è·¯çº¿1\n\n\nè·¯çº¿2\n\n\nè·¯çº¿3\n\n\næœ€ä½³è·¯çº¿\n\n\næœ€ä¼˜\n\n\nSå½¢\n\n\nç»„åˆ\n\n\næœ€å¤§é—´éš™\n\n\né€é€šé“\n\n\nç»„åˆ+"
            };

            translations["tr"] = new Dictionary<string, string>(translations["en"])
            {
                ["title"] = "EtkileÅŸimli Depo - Rota Planlama",
                ["language"] = "Dil",
                ["exit"] = "Ã‡Ä±kÄ±ÅŸ",
                ["exitTitle"] = "Ã‡Ä±kÄ±ÅŸ",
                ["exitMessage"] = "Ã‡Ä±kmak istediÄŸinizden emin misiniz?",
                ["setLayout"] = "Depo PlanÄ±",
                ["createOrder"] = "SipariÅŸ OluÅŸtur",
                ["createRoute"] = "Rota Ã‡iz",
                ["routingMethods"] = "Rota YÃ¶ntemleri",
                ["results"] = "SonuÃ§lar",
                ["layoutHelp"] = "Bu formu doldurun, sonra\nDepoyu GÃ¶ster'e tÄ±klayÄ±n.",
                ["blocks"] = "blok sayÄ±sÄ±",
                ["aisles"] = "koridor sayÄ±sÄ±",
                ["locations"] = "koridor baÅŸÄ±na\nlokasyon sayÄ±sÄ±",
                ["depotLocation"] = "depo konumu",
                ["aisleLength"] = "koridor uzunluÄŸu\n(opsiyonel)",
                ["crossoverLength"] = "ara koridor uzunluÄŸu\n(opsiyonel)",
                ["showWarehouse"] = "Depoyu GÃ¶ster",
                ["orderHelp"] = "Toplanacak Ã¼rÃ¼n sayÄ±sÄ±nÄ± girin,\nsonra rastgele sipariÅŸ oluÅŸturun.",
                ["manualOrderHelp"] = "Depodaki lokasyonlara tÄ±klayarak\nmanuel sipariÅŸ de oluÅŸturabilirsiniz.\nTÃ¼m seÃ§imleri temizlemek iÃ§in\nSipariÅŸi Temizle'ye tÄ±klayÄ±n.",
                ["setOrder"] = "SipariÅŸ OluÅŸtur",
                ["clearOrder"] = "SipariÅŸi Temizle",
                ["routeHelp"] = "Yeni Rota'ya tÄ±klayÄ±n, depot'tan\nbaÅŸlayÄ±p depoda rota Ã§izin.\nUlaÅŸÄ±lan Ã¼rÃ¼nler siyaha dÃ¶ner.",
                ["newRoute"] = "Yeni Rota",
                ["methodsHelp"] = "GeÃ§erli sipariÅŸ iÃ§in bir rota\nyÃ¶ntemi seÃ§in ve toplama\nrotasÄ±nÄ± gÃ¶rÃ¼ntÃ¼leyin.",
                ["resultLabels"] = "rota1\n\n\nrota2\n\n\nrota3\n\n\nen iyi rota\n\n\nOptimal\n\n\nS-shape\n\n\nCombined\n\n\nLargest Gap\n\n\nAisle-by-aisle\n\n\nCombined +"
            };

            translations["es"] = new Dictionary<string, string>(translations["en"])
            {
                ["title"] = "AlmacÃ©n Interactivo - PlanificaciÃ³n de Rutas",
                ["language"] = "Idioma",
                ["exit"] = "Salir",
                ["exitTitle"] = "Salir",
                ["exitMessage"] = "Â¿EstÃ¡ seguro de que desea salir?",
                ["setLayout"] = "Configurar DiseÃ±o",
                ["createOrder"] = "Crear Pedido",
                ["createRoute"] = "Crear Ruta",
                ["routingMethods"] = "MÃ©todos de Ruta",
                ["results"] = "Resultados",
                ["layoutHelp"] = "Rellene este formulario y luego\npulse Mostrar almacÃ©n.",
                ["blocks"] = "nÃºmero de bloques",
                ["aisles"] = "nÃºmero de pasillos",
                ["locations"] = "nÃºmero de ubicaciones\npor pasillo",
                ["depotLocation"] = "ubicaciÃ³n del depÃ³sito",
                ["aisleLength"] = "longitud del pasillo\n(opcional)",
                ["crossoverLength"] = "longitud del cruce\n(opcional)",
                ["showWarehouse"] = "Mostrar almacÃ©n",
                ["orderHelp"] = "Introduzca el nÃºmero de productos\na recoger y cree un pedido aleatorio.",
                ["manualOrderHelp"] = "TambiÃ©n puede crear un pedido\nmanualmente haciendo clic en\nlas ubicaciones. Pulse Limpiar pedido\npara reiniciar.",
                ["setOrder"] = "Crear Pedido",
                ["clearOrder"] = "Limpiar Pedido",
                ["routeHelp"] = "Pulse Nueva ruta, empiece desde\ndepot y dibuje la ruta.\nLos productos recogidos se vuelven negros.",
                ["newRoute"] = "Nueva Ruta",
                ["methodsHelp"] = "Seleccione un mÃ©todo para calcular\ny mostrar la ruta de recogida.",
                ["resultLabels"] = "ruta1\n\n\nruta2\n\n\nruta3\n\n\nmejor ruta\n\n\nÃ“ptima\n\n\nS-shape\n\n\nCombinada\n\n\nMayor hueco\n\n\nPasillo por pasillo\n\n\nCombinada +"
            };

            translations["ka"] = new Dictionary<string, string>(translations["en"])
            {
                ["title"] = "áƒ˜áƒœáƒ¢áƒ”áƒ áƒáƒ¥áƒ¢áƒ˜áƒ£áƒšáƒ˜ áƒ¡áƒáƒ¬áƒ§áƒáƒ‘áƒ˜ - áƒ›áƒáƒ áƒ¨áƒ áƒ£áƒ¢áƒ˜áƒ¡ áƒ“áƒáƒ’áƒ”áƒ’áƒ›áƒ•áƒ",
                ["language"] = "áƒ”áƒœáƒ",
                ["exit"] = "áƒ’áƒáƒ¡áƒ•áƒšáƒ",
                ["exitTitle"] = "áƒ’áƒáƒ¡áƒ•áƒšáƒ",
                ["exitMessage"] = "áƒ“áƒáƒ áƒ¬áƒ›áƒ£áƒœáƒ”áƒ‘áƒ£áƒšáƒ˜ áƒ®áƒáƒ áƒ—, áƒ áƒáƒ› áƒ’áƒ¡áƒ£áƒ áƒ— áƒ’áƒáƒ¡áƒ•áƒšáƒ?",
                ["setLayout"] = "áƒ’áƒáƒœáƒšáƒáƒ’áƒ”áƒ‘áƒ",
                ["createOrder"] = "áƒ¨áƒ”áƒ™áƒ•áƒ”áƒ—áƒ",
                ["createRoute"] = "áƒ›áƒáƒ áƒ¨áƒ áƒ£áƒ¢áƒ˜",
                ["routingMethods"] = "áƒ›áƒ”áƒ—áƒáƒ“áƒ”áƒ‘áƒ˜",
                ["results"] = "áƒ¨áƒ”áƒ“áƒ”áƒ’áƒ”áƒ‘áƒ˜",
                ["layoutHelp"] = "áƒ¨áƒ”áƒáƒ•áƒ¡áƒ”áƒ— áƒ¤áƒáƒ áƒ›áƒ áƒ“áƒ áƒ“áƒáƒáƒ­áƒ˜áƒ áƒ”áƒ—\náƒ¡áƒáƒ¬áƒ§áƒáƒ‘áƒ˜áƒ¡ áƒ©áƒ•áƒ”áƒœáƒ”áƒ‘áƒáƒ¡.",
                ["blocks"] = "áƒ‘áƒšáƒáƒ™áƒ”áƒ‘áƒ˜áƒ¡ áƒ áƒáƒáƒ“áƒ”áƒœáƒáƒ‘áƒ",
                ["aisles"] = "áƒ“áƒ”áƒ áƒ”áƒ¤áƒœáƒ”áƒ‘áƒ˜áƒ¡ áƒ áƒáƒáƒ“áƒ”áƒœáƒáƒ‘áƒ",
                ["locations"] = "áƒšáƒáƒ™áƒáƒªáƒ˜áƒ”áƒ‘áƒ˜áƒ¡ áƒ áƒáƒáƒ“áƒ”áƒœáƒáƒ‘áƒ\náƒ“áƒ”áƒ áƒ”áƒ¤áƒáƒœáƒ¨áƒ˜",
                ["depotLocation"] = "áƒ“áƒ”áƒáƒáƒ¡ áƒ›áƒ“áƒ”áƒ‘áƒáƒ áƒ”áƒáƒ‘áƒ",
                ["aisleLength"] = "áƒ“áƒ”áƒ áƒ”áƒ¤áƒœáƒ˜áƒ¡ áƒ¡áƒ˜áƒ’áƒ áƒ«áƒ”\n(áƒáƒ áƒáƒ¡áƒáƒ•áƒáƒšáƒ“áƒ”áƒ‘áƒ£áƒšáƒ)",
                ["crossoverLength"] = "áƒ’áƒáƒ“áƒáƒ™áƒ•áƒ”áƒ—áƒ˜áƒ¡ áƒ¡áƒ˜áƒ’áƒ áƒ«áƒ”\n(áƒáƒ áƒáƒ¡áƒáƒ•áƒáƒšáƒ“áƒ”áƒ‘áƒ£áƒšáƒ)",
                ["showWarehouse"] = "áƒ¡áƒáƒ¬áƒ§áƒáƒ‘áƒ˜áƒ¡ áƒ©áƒ•áƒ”áƒœáƒ”áƒ‘áƒ",
                ["orderHelp"] = "áƒ¨áƒ”áƒ˜áƒ§áƒ•áƒáƒœáƒ”áƒ— áƒáƒ¡áƒáƒ¦áƒ”áƒ‘áƒ˜ áƒáƒ áƒáƒ“áƒ£áƒ¥áƒ¢áƒ”áƒ‘áƒ˜áƒ¡\náƒ áƒáƒáƒ“áƒ”áƒœáƒáƒ‘áƒ áƒ“áƒ áƒ¨áƒ”áƒ¥áƒ›áƒ”áƒœáƒ˜áƒ— áƒ¨áƒ”áƒ™áƒ•áƒ”áƒ—áƒ.",
                ["manualOrderHelp"] = "áƒ¨áƒ”áƒ™áƒ•áƒ”áƒ—áƒ˜áƒ¡ áƒ®áƒ”áƒšáƒ˜áƒ— áƒ¨áƒ”áƒ¥áƒ›áƒœáƒáƒª áƒ¨áƒ”áƒ’áƒ˜áƒ«áƒšáƒ˜áƒáƒ—\náƒšáƒáƒ™áƒáƒªáƒ˜áƒ”áƒ‘áƒ–áƒ” áƒ“áƒáƒ¬áƒ™áƒáƒáƒ”áƒ‘áƒ˜áƒ—. áƒ’áƒáƒ¡áƒáƒ¡áƒ£áƒ¤áƒ—áƒáƒ•áƒ”áƒ‘áƒšáƒáƒ“\náƒ“áƒáƒáƒ­áƒ˜áƒ áƒ”áƒ— áƒ’áƒáƒ¡áƒ£áƒ¤áƒ—áƒáƒ•áƒ”áƒ‘áƒáƒ¡.",
                ["setOrder"] = "áƒ¨áƒ”áƒ™áƒ•áƒ”áƒ—áƒ˜áƒ¡ áƒ¨áƒ”áƒ¥áƒ›áƒœáƒ",
                ["clearOrder"] = "áƒ’áƒáƒ¡áƒ£áƒ¤áƒ—áƒáƒ•áƒ”áƒ‘áƒ",
                ["routeHelp"] = "áƒ“áƒáƒáƒ­áƒ˜áƒ áƒ”áƒ— áƒáƒ®áƒáƒš áƒ›áƒáƒ áƒ¨áƒ áƒ£áƒ¢áƒ¡,\náƒ“áƒáƒ˜áƒ¬áƒ§áƒ”áƒ— depot-áƒ“áƒáƒœ áƒ“áƒ áƒ“áƒáƒ®áƒáƒ–áƒ”áƒ— áƒ’áƒ–áƒ.\náƒáƒ¦áƒ”áƒ‘áƒ£áƒšáƒ˜ áƒáƒ áƒáƒ“áƒ£áƒ¥áƒ¢áƒ”áƒ‘áƒ˜ áƒ¨áƒáƒ•áƒ“áƒ”áƒ‘áƒ.",
                ["newRoute"] = "áƒáƒ®áƒáƒšáƒ˜ áƒ›áƒáƒ áƒ¨áƒ áƒ£áƒ¢áƒ˜",
                ["methodsHelp"] = "áƒáƒ˜áƒ áƒ©áƒ˜áƒ”áƒ— áƒ›áƒ”áƒ—áƒáƒ“áƒ˜ áƒ›áƒ˜áƒ›áƒ“áƒ˜áƒœáƒáƒ áƒ” áƒ¨áƒ”áƒ™áƒ•áƒ”áƒ—áƒ˜áƒ¡\náƒ›áƒáƒ áƒ¨áƒ áƒ£áƒ¢áƒ˜áƒ¡ áƒ’áƒáƒ›áƒáƒ¡áƒáƒ—áƒ•áƒšáƒ”áƒšáƒáƒ“.",
                ["resultLabels"] = "áƒ›áƒáƒ áƒ¨áƒ áƒ£áƒ¢áƒ˜1\n\n\náƒ›áƒáƒ áƒ¨áƒ áƒ£áƒ¢áƒ˜2\n\n\náƒ›áƒáƒ áƒ¨áƒ áƒ£áƒ¢áƒ˜3\n\n\náƒ¡áƒáƒ£áƒ™áƒ”áƒ—áƒ”áƒ¡áƒ\n\n\náƒáƒáƒ¢áƒ˜áƒ›áƒáƒšáƒ£áƒ áƒ˜\n\n\nS-shape\n\n\nCombined\n\n\nLargest Gap\n\n\nAisle-by-aisle\n\n\nCombined +"
            };
        }

        private string T(string key)
        {
            if (translations.ContainsKey(currentLanguage) &&
                translations[currentLanguage].ContainsKey(key))
                return translations[currentLanguage][key];

            return translations["en"].ContainsKey(key) ? translations["en"][key] : key;
        }

        private void OverrideCleanTranslations()
        {
            translations["en"] = new Dictionary<string, string>
            {
                ["title"] = "Interactive Warehouse - Route Planning",
                ["language"] = "Language",
                ["exit"] = "Exit",
                ["exitTitle"] = "Exit",
                ["exitMessage"] = "Are you sure you want to exit?",
                ["setLayout"] = "Set Layout",
                ["createOrder"] = "Create an Order",
                ["createRoute"] = "Create a Route",
                ["routingMethods"] = "Routing Methods",
                ["results"] = "Results",
                ["layoutHelp"] = "Fill out this form, then click\nShow Warehouse.",
                ["blocks"] = "blocks",
                ["aisles"] = "aisles",
                ["locations"] = "locations per aisle",
                ["depotLocation"] = "depot location",
                ["aisleLength"] = "aisle length (optional)",
                ["crossoverLength"] = "crossover length (optional)",
                ["shelfCount"] = "shelves",
                ["warehouseHeight"] = "height",
                ["shelfSelector"] = "Shelf layer",
                ["currentShelf"] = "Current shelf",
                ["vehicle"] = "Vehicle",
                ["vehicleHuman"] = "Human",
                ["routeOptimal"] = "Optimal",
                ["routeSShape"] = "S-shape",
                ["routeCombined"] = "Combined",
                ["routeLargestGap"] = "Largest Gap",
                ["routeAisleByAisle"] = "Aisle-by-aisle",
                ["routeCombinedPlus"] = "Combined +",
                ["showWarehouse"] = "Show Warehouse",
                ["orderHelp"] = "Enter the number of products\nto pick, then create a random\norder.",
                ["manualOrderHelp"] = "You can also create an order\nmanually by clicking warehouse\nlocations. Click Clear Order to\nreset all selections.",
                ["setOrder"] = "Set Order",
                ["clearOrder"] = "Clear Order",
                ["routeHelp"] = "Click New Route, then start\nfrom depot and draw a route.\nSelected products turn black\nwhen the route reaches them.",
                ["newRoute"] = "New Route",
                ["methodsHelp"] = "Select a routing method to\ncalculate and display a picking\nroute for the current order.",
                ["resultLabels"] = "route1\n\n\nroute2\n\n\nroute3\n\n\nyour best route\n\n\nOptimal\n\n\nS-shape\n\n\nCombined\n\n\nLargest Gap\n\n\nAisle-by-aisle\n\n\nCombined +"
            };

            translations["tr"] = new Dictionary<string, string>(translations["en"])
            {
                ["title"] = "Depo Rota Planlama",
                ["language"] = "Dil",
                ["exit"] = "Cikis",
                ["exitTitle"] = "Cikis",
                ["exitMessage"] = "Cikmak istediginizden emin misiniz?",
                ["setLayout"] = "Depo Plani",
                ["createOrder"] = "Siparis Olustur",
                ["createRoute"] = "Rota Ciz",
                ["routingMethods"] = "Rota Yontemleri",
                ["results"] = "Sonuclar",
                ["layoutHelp"] = "Bu formu doldurun, sonra\nDepoyu Goster'e tiklayin.",
                ["blocks"] = "blok sayisi",
                ["aisles"] = "koridor sayisi",
                ["locations"] = "lokasyon/koridor",
                ["depotLocation"] = "depo konumu",
                ["aisleLength"] = "koridor uzunlugu",
                ["crossoverLength"] = "ara koridor uzunlugu",
                ["shelfCount"] = "raf sayisi",
                ["warehouseHeight"] = "yukseklik",
                ["shelfSelector"] = "Raf katmani",
                ["currentShelf"] = "Mevcut raf",
                ["vehicle"] = "Arac",
                ["vehicleHuman"] = "Insan",
                ["routeOptimal"] = "En iyi",
                ["routeSShape"] = "S sekli",
                ["routeCombined"] = "Birlesik",
                ["routeLargestGap"] = "En buyuk bosluk",
                ["routeAisleByAisle"] = "Koridor koridor",
                ["routeCombinedPlus"] = "Birlesik +",
                ["showWarehouse"] = "Depoyu Goster",
                ["orderHelp"] = "Toplanacak urun sayisini girin,\nsonra rastgele siparis olusturun.",
                ["manualOrderHelp"] = "Depodaki lokasyonlara tiklayarak\nmanuel siparis de olusturabilirsiniz.\nTum secimleri temizlemek icin\nSiparisi Temizle'ye tiklayin.",
                ["setOrder"] = "Siparis Olustur",
                ["clearOrder"] = "Siparisi Temizle",
                ["routeHelp"] = "Yeni Rota'ya tiklayin, depot'tan\nbaslayip depoda rota cizin.\nUlasilan urunler siyaha doner.",
                ["newRoute"] = "Yeni Rota",
                ["methodsHelp"] = "Gecerli siparis icin bir rota\nyontemi secin ve toplama\nrotasini goruntuleyin.",
                ["resultLabels"] = "rota1\n\n\nrota2\n\n\nrota3\n\n\nen iyi rota\n\n\nOptimal\n\n\nS-shape\n\n\nCombined\n\n\nLargest Gap\n\n\nAisle-by-aisle\n\n\nCombined +"
            };

            translations["es"] = new Dictionary<string, string>(translations["en"])
            {
                ["title"] = "Almacen Interactivo - Rutas",
                ["language"] = "Idioma",
                ["exit"] = "Salir",
                ["exitMessage"] = "Esta seguro de que desea salir?",
                ["setLayout"] = "Diseno",
                ["createOrder"] = "Crear Pedido",
                ["createRoute"] = "Crear Ruta",
                ["routingMethods"] = "Metodos",
                ["results"] = "Resultados",
                ["showWarehouse"] = "Mostrar almacen",
                ["shelfCount"] = "estantes",
                ["warehouseHeight"] = "altura",
                ["shelfSelector"] = "Nivel",
                ["currentShelf"] = "Estante actual",
                ["vehicle"] = "Vehiculo",
                ["vehicleHuman"] = "Humano",
                ["routeOptimal"] = "Optimo",
                ["routeSShape"] = "Forma S",
                ["routeCombined"] = "Combinado",
                ["routeLargestGap"] = "Mayor hueco",
                ["routeAisleByAisle"] = "Pasillo por pasillo",
                ["routeCombinedPlus"] = "Combinado +"
            };

            translations["zh"] = new Dictionary<string, string>(translations["en"])
            {
                ["title"] = "\u4ea4\u4e92\u5f0f\u4ed3\u5e93 - \u8def\u7ebf\u89c4\u5212",
                ["language"] = "\u8bed\u8a00",
                ["exit"] = "\u9000\u51fa",
                ["setLayout"] = "\u5e03\u5c40",
                ["createOrder"] = "\u8ba2\u5355",
                ["createRoute"] = "\u8def\u7ebf",
                ["routingMethods"] = "\u65b9\u6cd5",
                ["results"] = "\u7ed3\u679c",
                ["shelfCount"] = "\u8d27\u67b6",
                ["warehouseHeight"] = "\u9ad8\u5ea6",
                ["shelfSelector"] = "\u5c42",
                ["currentShelf"] = "\u5f53\u524d\u5c42",
                ["vehicle"] = "\u8f66\u8f86",
                ["vehicleHuman"] = "\u4eba",
                ["routeOptimal"] = "\u6700\u4f18",
                ["routeSShape"] = "S\u5f62",
                ["routeCombined"] = "\u7ec4\u5408",
                ["routeLargestGap"] = "\u6700\u5927\u95f4\u9699",
                ["routeAisleByAisle"] = "\u9010\u901a\u9053",
                ["routeCombinedPlus"] = "\u7ec4\u5408 +",
                ["layoutHelp"] = "\u586b\u5199\u6b64\u8868\u5355\uff0c\u7136\u540e\u70b9\u51fb\n\u663e\u793a\u4ed3\u5e93\u3002",
                ["blocks"] = "\u533a\u5757",
                ["aisles"] = "\u901a\u9053",
                ["locations"] = "\u6bcf\u901a\u9053\u4f4d\u7f6e",
                ["depotLocation"] = "\u4ed3\u5e93\u53e3\u4f4d\u7f6e",
                ["aisleLength"] = "\u901a\u9053\u957f\u5ea6\uff08\u53ef\u9009\uff09",
                ["crossoverLength"] = "\u6a2a\u5411\u901a\u9053\u957f\u5ea6\uff08\u53ef\u9009\uff09",
                ["showWarehouse"] = "\u663e\u793a\u4ed3\u5e93",
                ["orderHelp"] = "\u8f93\u5165\u8981\u62e3\u9009\u7684\u4ea7\u54c1\u6570\u91cf\uff0c\n\u7136\u540e\u521b\u5efa\u968f\u673a\u8ba2\u5355\u3002",
                ["manualOrderHelp"] = "\u4e5f\u53ef\u4ee5\u70b9\u51fb\u4ed3\u5e93\u4f4d\u7f6e\n\u624b\u52a8\u521b\u5efa\u8ba2\u5355\u3002\u70b9\u51fb\u6e05\u9664\u8ba2\u5355\n\u53ef\u91cd\u7f6e\u6240\u6709\u9009\u62e9\u3002",
                ["setOrder"] = "\u8bbe\u7f6e\u8ba2\u5355",
                ["clearOrder"] = "\u6e05\u9664\u8ba2\u5355",
                ["routeHelp"] = "\u70b9\u51fb\u65b0\u8def\u7ebf\uff0c\u7136\u540e\u4ece\u4ed3\u5e93\u53e3\n\u5f00\u59cb\u7ed8\u5236\u8def\u7ebf\u3002\n\u5230\u8fbe\u7684\u4ea7\u54c1\u4f1a\u53d8\u4e3a\u9ed1\u8272\u3002",
                ["newRoute"] = "\u65b0\u8def\u7ebf",
                ["methodsHelp"] = "\u9009\u62e9\u4e00\u79cd\u8def\u7ebf\u65b9\u6cd5\uff0c\n\u8ba1\u7b97\u5e76\u663e\u793a\u5f53\u524d\u8ba2\u5355\u7684\n\u62e3\u9009\u8def\u7ebf\u3002",
                ["resultLabels"] = "\u8def\u7ebf1\n\n\n\u8def\u7ebf2\n\n\n\u8def\u7ebf3\n\n\n\u6700\u4f73\u8def\u7ebf\n\n\n\u6700\u4f18\n\n\nS\u5f62\n\n\n\u7ec4\u5408\n\n\n\u6700\u5927\u95f4\u9699\n\n\n\u9010\u901a\u9053\n\n\n\u7ec4\u5408 +"
            };

            translations["ka"] = new Dictionary<string, string>(translations["en"])
            {
                ["title"] = "\u10e1\u10d0\u10ec\u10e7\u10dd\u10d1\u10d8\u10e1 \u10db\u10d0\u10e0\u10e8\u10e0\u10e3\u10e2\u10d8\u10e1 \u10d3\u10d0\u10d2\u10d4\u10d2\u10db\u10d5\u10d0",
                ["language"] = "\u10d4\u10dc\u10d0",
                ["exit"] = "\u10d3\u10d0\u10ee\u10e3\u10e0\u10d5\u10d0",
                ["exitTitle"] = "\u10d2\u10d0\u10e1\u10d5\u10da\u10d0",
                ["exitMessage"] = "\u10d3\u10d0\u10e0\u10ec\u10db\u10e3\u10dc\u10d4\u10d1\u10e3\u10da\u10d8 \u10ee\u10d0\u10e0\u10d7, \u10e0\u10dd\u10db \u10d2\u10e1\u10e3\u10e0\u10d7 \u10d2\u10d0\u10e1\u10d5\u10da\u10d0?",
                ["setLayout"] = "\u10d2\u10d0\u10dc\u10da\u10d0\u10d2\u10d4\u10d1\u10d0",
                ["createOrder"] = "\u10e8\u10d4\u10d9\u10d5\u10d4\u10d7\u10d0",
                ["createRoute"] = "\u10db\u10d0\u10e0\u10e8\u10e0\u10e3\u10e2\u10d8",
                ["routingMethods"] = "\u10db\u10d4\u10d7\u10dd\u10d3\u10d4\u10d1\u10d8",
                ["results"] = "\u10e8\u10d4\u10d3\u10d4\u10d2\u10d4\u10d1\u10d8",
                ["layoutHelp"] = "\u10e8\u10d4\u10d0\u10d5\u10e1\u10d4\u10d7 \u10e4\u10dd\u10e0\u10db\u10d0 \u10d3\u10d0 \u10d3\u10d0\u10d0\u10ed\u10d8\u10e0\u10d4\u10d7\n\u10e1\u10d0\u10ec\u10e7\u10dd\u10d1\u10d8\u10e1 \u10e9\u10d5\u10d4\u10dc\u10d4\u10d1\u10d0\u10e1.",
                ["blocks"] = "\u10d1\u10da\u10dd\u10d9\u10d4\u10d1\u10d8",
                ["aisles"] = "\u10d3\u10d4\u10e0\u10d4\u10e4\u10dc\u10d4\u10d1\u10d8",
                ["locations"] = "\u10d0\u10d3\u10d2\u10d8\u10da\u10d4\u10d1\u10d8 \u10d3\u10d4\u10e0\u10d4\u10e4\u10d0\u10dc\u10e8\u10d8",
                ["depotLocation"] = "\u10d3\u10d4\u10de\u10dd\u10e1 \u10db\u10d3\u10d4\u10d1\u10d0\u10e0\u10d4\u10dd\u10d1\u10d0",
                ["aisleLength"] = "\u10d3\u10d4\u10e0\u10d4\u10e4\u10dc\u10d8\u10e1 \u10e1\u10d8\u10d2\u10e0\u10eb\u10d4 (\u10d0\u10e0\u10d0\u10e1\u10d0\u10d5\u10d0\u10da\u10d3\u10d4\u10d1\u10e3\u10da\u10dd)",
                ["crossoverLength"] = "\u10d2\u10d0\u10db\u10e1\u10d5\u10da\u10d4\u10da\u10d8\u10e1 \u10e1\u10d8\u10d2\u10e0\u10eb\u10d4 (\u10d0\u10e0\u10d0\u10e1\u10d0\u10d5\u10d0\u10da\u10d3\u10d4\u10d1\u10e3\u10da\u10dd)",
                ["shelfCount"] = "\u10d7\u10d0\u10e0\u10dd\u10d4\u10d1\u10d8\u10e1 \u10e0\u10d0\u10dd\u10d3\u10d4\u10dc\u10dd\u10d1\u10d0",
                ["warehouseHeight"] = "\u10e1\u10d8\u10db\u10d0\u10e6\u10da\u10d4",
                ["shelfSelector"] = "\u10d7\u10d0\u10e0\u10dd\u10e1 \u10e4\u10d4\u10dc\u10d0",
                ["currentShelf"] = "\u10db\u10d8\u10db\u10d3\u10d8\u10dc\u10d0\u10e0\u10d4 \u10d7\u10d0\u10e0\u10dd",
                ["vehicle"] = "\u10e2\u10e0\u10d0\u10dc\u10e1\u10de\u10dd\u10e0\u10e2\u10d8",
                ["vehicleHuman"] = "\u10d0\u10d3\u10d0\u10db\u10d8\u10d0\u10dc\u10d8",
                ["showWarehouse"] = "\u10e1\u10d0\u10ec\u10e7\u10dd\u10d1\u10d8\u10e1 \u10e9\u10d5\u10d4\u10dc\u10d4\u10d1\u10d0",
                ["orderHelp"] = "\u10e8\u10d4\u10d8\u10e7\u10d5\u10d0\u10dc\u10d4\u10d7 \u10d0\u10e1\u10d0\u10e6\u10d4\u10d1\u10d8 \u10de\u10e0\u10dd\u10d3\u10e3\u10e5\u10e2\u10d4\u10d1\u10d8\u10e1 \u10e0\u10d0\u10dd\u10d3\u10d4\u10dc\u10dd\u10d1\u10d0,\n\u10e8\u10d4\u10db\u10d3\u10d4\u10d2 \u10e8\u10d4\u10e5\u10db\u10d4\u10dc\u10d8\u10d7 \u10e8\u10d4\u10db\u10d7\u10ee\u10d5\u10d4\u10d5\u10d8\u10d7\u10d8 \u10e8\u10d4\u10d9\u10d5\u10d4\u10d7\u10d0.",
                ["manualOrderHelp"] = "\u10e8\u10d4\u10d9\u10d5\u10d4\u10d7\u10d8\u10e1 \u10ee\u10d4\u10da\u10d8\u10d7 \u10e8\u10d4\u10e5\u10db\u10dc\u10d0\u10ea \u10e8\u10d4\u10d2\u10d8\u10eb\u10da\u10d8\u10d0\u10d7\n\u10e1\u10d0\u10ec\u10e7\u10dd\u10d1\u10d8\u10e1 \u10d0\u10d3\u10d2\u10d8\u10da\u10d4\u10d1\u10d6\u10d4 \u10d3\u10d0\u10ed\u10d4\u10e0\u10d8\u10d7.\n\u10d2\u10d0\u10e1\u10d0\u10e1\u10e3\u10e4\u10d7\u10d0\u10d5\u10d4\u10d1\u10da\u10d0\u10d3 \u10d3\u10d0\u10d0\u10ed\u10d8\u10e0\u10d4\u10d7 \u10d2\u10d0\u10e1\u10e3\u10e4\u10d7\u10d0\u10d5\u10d4\u10d1\u10d0\u10e1.",
                ["setOrder"] = "\u10e8\u10d4\u10d9\u10d5\u10d4\u10d7\u10d8\u10e1 \u10d3\u10d0\u10e7\u10d4\u10dc\u10d4\u10d1\u10d0",
                ["clearOrder"] = "\u10e8\u10d4\u10d9\u10d5\u10d4\u10d7\u10d8\u10e1 \u10d2\u10d0\u10e1\u10e3\u10e4\u10d7\u10d0\u10d5\u10d4\u10d1\u10d0",
                ["routeHelp"] = "\u10d3\u10d0\u10d0\u10ed\u10d8\u10e0\u10d4\u10d7 \u10d0\u10ee\u10d0\u10da \u10db\u10d0\u10e0\u10e8\u10e0\u10e3\u10e2\u10e1,\n\u10d3\u10d0\u10d8\u10ec\u10e7\u10d4\u10d7 \u10d3\u10d4\u10de\u10dd\u10d3\u10d0\u10dc \u10d3\u10d0 \u10d3\u10d0\u10ee\u10d0\u10d6\u10d4\u10d7 \u10d2\u10d6\u10d0.\n\u10d0\u10e6\u10d4\u10d1\u10e3\u10da\u10d8 \u10de\u10e0\u10dd\u10d3\u10e3\u10e5\u10e2\u10d4\u10d1\u10d8 \u10d2\u10d0\u10e8\u10d0\u10d5\u10d3\u10d4\u10d1\u10d0.",
                ["newRoute"] = "\u10d0\u10ee\u10d0\u10da\u10d8 \u10db\u10d0\u10e0\u10e8\u10e0\u10e3\u10e2\u10d8",
                ["methodsHelp"] = "\u10d0\u10d8\u10e0\u10e9\u10d8\u10d4\u10d7 \u10db\u10d0\u10e0\u10e8\u10e0\u10e3\u10e2\u10d8\u10e1 \u10db\u10d4\u10d7\u10dd\u10d3\u10d8,\n\u10e0\u10dd\u10db \u10d8\u10dc\u10d0\u10ee\u10dd\u10d7 \u10db\u10d8\u10db\u10d3\u10d8\u10dc\u10d0\u10e0\u10d4 \u10e8\u10d4\u10d9\u10d5\u10d4\u10d7\u10d8\u10e1\n\u10d0\u10e1\u10d0\u10e6\u10d4\u10d1\u10d8 \u10db\u10d0\u10e0\u10e8\u10e0\u10e3\u10e2\u10d8.",
                ["resultLabels"] = "\u10db\u10d0\u10e0\u10e8\u10e0\u10e3\u10e2\u10d8 1\n\n\n\u10db\u10d0\u10e0\u10e8\u10e0\u10e3\u10e2\u10d8 2\n\n\n\u10db\u10d0\u10e0\u10e8\u10e0\u10e3\u10e2\u10d8 3\n\n\n\u10e1\u10d0\u10e3\u10d9\u10d4\u10d7\u10d4\u10e1\u10dd \u10db\u10d0\u10e0\u10e8\u10e0\u10e3\u10e2\u10d8\n\n\n\u10dd\u10de\u10e2\u10d8\u10db\u10d0\u10da\u10e3\u10e0\u10d8\n\n\nS-\u10e4\u10dd\u10e0\u10db\u10d0\n\n\n\u10d2\u10d0\u10d4\u10e0\u10d7\u10d8\u10d0\u10dc\u10d4\u10d1\u10e3\u10da\u10d8\n\n\n\u10e3\u10d3\u10d8\u10d3\u10d4\u10e1\u10d8 \u10ee\u10d0\u10e0\u10d5\u10d4\u10d6\u10d8\n\n\n\u10d3\u10d4\u10e0\u10d4\u10e4\u10d0\u10dc-\u10d3\u10d4\u10e0\u10d4\u10e4\u10d0\u10dc\n\n\n\u10d2\u10d0\u10d4\u10e0\u10d7\u10d8\u10d0\u10dc\u10d4\u10d1\u10e3\u10da\u10d8 +",
                ["routeOptimal"] = "\u10dd\u10de\u10e2\u10d8\u10db\u10d0\u10da\u10e3\u10e0\u10d8",
                ["routeSShape"] = "S-\u10e4\u10dd\u10e0\u10db\u10d0",
                ["routeCombined"] = "\u10d2\u10d0\u10d4\u10e0\u10d7\u10d8\u10d0\u10dc\u10d4\u10d1\u10e3\u10da\u10d8",
                ["routeLargestGap"] = "\u10e3\u10d3\u10d8\u10d3\u10d4\u10e1\u10d8 \u10ee\u10d0\u10e0\u10d5\u10d4\u10d6\u10d8",
                ["routeAisleByAisle"] = "\u10d3\u10d4\u10e0\u10d4\u10e4\u10d0\u10dc-\u10d3\u10d4\u10e0\u10d4\u10e4\u10d0\u10dc",
                ["routeCombinedPlus"] = "\u10d2\u10d0\u10d4\u10e0\u10d7\u10d8\u10d0\u10dc\u10d4\u10d1\u10e3\u10da\u10d8 +"
            };
        }

        private void ApplyLanguage(string language)
        {
            currentLanguage = language;

            Text = T("title");
            if (lblLanguage != null) lblLanguage.Text = T("language");
            if (btnExit != null) btnExit.Text = T("exit");

            radioButton1.Text = T("setLayout");
            radioButton2.Text = T("createOrder");
            radioButton3.Text = T("createRoute");
            radioButton4.Text = T("routingMethods");
            radioButton5.Text = T("results");

            label1.Text = T("setLayout").ToUpper();
            label2.Text = T("layoutHelp");
            label3.Text = T("blocks");
            label4.Text = T("aisles");
            label5.Text = T("locations");
            label6.Text = T("depotLocation");
            label7.Text = T("aisleLength");
            label8.Text = T("crossoverLength");
            label18.Text = T("shelfCount");
            label19.Text = T("warehouseHeight");
            if (lblShelfSelector != null) lblShelfSelector.Text = T("shelfSelector");
            if (lblVehicle != null) lblVehicle.Text = T("vehicle");
            UpdateVehicleItems();
            btnShowWarehouse.Text = T("showWarehouse");
            UpdateCurrentShelfLabel();

            label9.Text = T("createOrder").ToUpper();
            label10.Text = T("orderHelp");
            label11.Text = T("manualOrderHelp");
            button2.Text = T("setOrder");
            button3.Text = T("clearOrder");

            label14.Text = T("createRoute").ToUpper();
            label12.Text = T("routeHelp");
            button4.Text = T("newRoute");

            label15.Text = T("routingMethods").ToUpper();
            label13.Text = T("methodsHelp");

            label17.Text = T("results").ToUpper();
            label16.Text = T("resultLabels");

            button5.Text = T("routeOptimal");
            button6.Text = T("routeSShape");
            button7.Text = T("routeCombined");
            button8.Text = T("routeLargestGap");
            button9.Text = T("routeAisleByAisle");
            button10.Text = T("routeCombinedPlus");
            ApplyRolePermissions();
        }

        private void ApplyRolePermissions()
        {
            bool canOperate = !string.Equals(loggedInRole, "Viewer", StringComparison.OrdinalIgnoreCase);

            foreach (Button button in new[] { button2, button3, button4, button5, button6, button7, button8, button9, button10 })
                button.Enabled = canOperate;
        }

        private void StyleButtons(Control parent)
        {
            foreach (Control control in parent.Controls)
            {
                if (control is Button button)
                {
                    button.FlatStyle = FlatStyle.Flat;
                    button.FlatAppearance.BorderSize = 0;
                    button.BackColor = Color.FromArgb(25, 118, 210);
                    button.ForeColor = Color.White;
                    button.Font = new Font("Segoe UI", 8.5F, FontStyle.Bold);
                    button.Height = Math.Max(button.Height, 28);
                }

                if (control is TextBox textBox)
                {
                    textBox.BorderStyle = BorderStyle.FixedSingle;
                    textBox.BackColor = Color.FromArgb(250, 252, 255);
                }

                if (control.HasChildren)
                    StyleButtons(control);
            }
        }

        private void SetDefaultInputs()
        {
            txtBlocks.Text = "2";
            txtAisles.Text = "5";
            txtLocations.Text = "8";
            txtDepot.Text = "1";
            txtAisleLength.Text = "";
            txtCrossoverLength.Text = "";
            textBox1.Text = "1";
            textBox2.Text = "";
            textBox7.Text = "8";
            PopulateShelfSelector();
        }

        private void RunSShapeRoute(string routeName)
        {
            activeRouteName = routeName;
            button6_Click(this, EventArgs.Empty);
        }

        private void RunCombinedRoute(string routeName)
        {
            activeRouteName = routeName;
            button7_Click(this, EventArgs.Empty);
        }

        private void RunOptimalRoute()
        {
            button5_Click(this, EventArgs.Empty);
        }

        private void StoreRouteResult(string defaultRouteName)
        {
            string routeName = activeRouteName ?? defaultRouteName;
            activeRouteName = null;

            if (rotaYolu.Count < 2)
                return;

            double distance = CalculateRouteDistance();
            routeResults[routeName] = distance;
            StatsService.Record(loggedInUser, routeName, Math.Max(GetPickedOrderCount(), GetSelectedOrderCount()), GetSelectedVehicle(), (decimal)distance);
            UpdateResultsPanel();
        }

        private void FinishManualRoute()
        {
            if (!cizimModu || rotaYolu.Count < 2)
                return;

            cizimModu = false;
            string routeName = "route" + manualRouteNumber;
            manualRouteNumber = manualRouteNumber >= 3 ? 1 : manualRouteNumber + 1;
            double distance = CalculateRouteDistance();
            routeResults[routeName] = distance;
            StatsService.Record(loggedInUser, routeName, Math.Max(GetPickedOrderCount(), GetSelectedOrderCount()), GetSelectedVehicle(), (decimal)distance);
            UpdateResultsPanel();
            panelWarehouse.Invalidate();
        }

        private string GetSelectedVehicle()
        {
            if (cmbVehicle == null || cmbVehicle.SelectedItem == null)
                return "Human";

            return StatsService.NormalizeVehicle(cmbVehicle.SelectedItem.ToString());
        }

        private void UpdateVehicleItems()
        {
            if (cmbVehicle == null)
                return;

            string selectedVehicle = GetSelectedVehicle();
            cmbVehicle.Items.Clear();
            cmbVehicle.Items.Add(T("vehicleHuman"));
            cmbVehicle.Items.Add("Forklift");
            cmbVehicle.Items.Add("Robot");

            if (selectedVehicle == "Forklift")
                cmbVehicle.SelectedIndex = 1;
            else if (selectedVehicle == "Robot")
                cmbVehicle.SelectedIndex = 2;
            else
                cmbVehicle.SelectedIndex = 0;
        }

        private double CalculateRouteDistance()
        {
            double distance = 0;

            for (int i = 0; i < rotaYolu.Count - 1; i++)
            {
                distance += Math.Abs(rotaYolu[i + 1].X - rotaYolu[i].X);
                distance += Math.Abs(rotaYolu[i + 1].Y - rotaYolu[i].Y);
            }

            return Math.Round(distance / Math.Max(CellW, 1), 1);
        }

        private void UpdateResultsPanel()
        {
            textBox8.Text = GetResultText("route1");
            textBox9.Text = GetResultText("route2");
            textBox10.Text = GetResultText("route3");
            textBox11.Text = routeResults.Count == 0
                ? ""
                : routeResults.OrderBy(r => r.Value).First().Key + " (" + routeResults.OrderBy(r => r.Value).First().Value + ")";
            textBox12.Text = GetResultText("Optimal");
            textBox13.Text = GetResultText("S-shape");
            textBox14.Text = GetResultText("Combined");
            textBox15.Text = GetResultText("Largest Gap");
            textBox16.Text = GetResultText("Aisle-by-aisle");
            textBox17.Text = GetResultText("Combined +");
        }

        private string GetResultText(string routeName)
        {
            return routeResults.ContainsKey(routeName)
                ? routeResults[routeName].ToString("0.0")
                : "";
        }

        private Dictionary<int, List<Rectangle>> GetSelectedItemsByNearestAisle(List<int> aisleXs)
        {
            Dictionary<int, List<Rectangle>> aisleItems = new Dictionary<int, List<Rectangle>>();

            foreach (var cell in hucreler.Where(h => h.IsSelected))
            {
                int cellCenterX = cell.Bounds.X + CellW / 2;
                int nearestAisleX = aisleXs
                    .OrderBy(x => Math.Abs(x - cellCenterX))
                    .First();

                if (!aisleItems.ContainsKey(nearestAisleX))
                    aisleItems[nearestAisleX] = new List<Rectangle>();

                aisleItems[nearestAisleX].Add(cell.Bounds);
            }

            return aisleItems;
        }

        // ==========================================
        // PANEL GEÃ‡Ä°ÅLERÄ°
        // ==========================================
        private void radioButton1_CheckedChanged(object sender, EventArgs e)
        {
            if (radioButton1.Checked)
                GizleGoster(pnlLayout);
        }

        private void radioButton2_CheckedChanged(object sender, EventArgs e)
        {
            if (radioButton2.Checked)
                GizleGoster(pnlOrder);
        }

        private void radioButton3_CheckedChanged(object sender, EventArgs e)
        {
            if (radioButton3.Checked)
                GizleGoster(pnlRoute);
        }

        private void radioButton4_CheckedChanged(object sender, EventArgs e)
        {
            if (radioButton4.Checked)
                GizleGoster(pnlMethods);
        }

        private void radioButton5_CheckedChanged(object sender, EventArgs e)
        {
            if (radioButton5.Checked)
                GizleGoster(pnlResults);
        }

        private void GizleGoster(Panel gosterilecekPanel)
        {
            pnlLayout.Visible = false;
            pnlOrder.Visible = false;
            pnlRoute.Visible = false;
            pnlMethods.Visible = false;
            pnlResults.Visible = false;

            gosterilecekPanel.Visible = true;
        }

        // ==========================================
        // DEPOYU GÃ–STER
        // ==========================================
        private void btnShowWarehouse_Click(object sender, EventArgs e)
        {
            int.TryParse(txtBlocks.Text, out blocks);
            int.TryParse(txtAisles.Text, out aisles);
            int.TryParse(txtLocations.Text, out locations);
            int.TryParse(txtDepot.Text, out depot);
            int.TryParse(txtAisleLength.Text, out optionalAisleLength);
            int.TryParse(txtCrossoverLength.Text, out optionalCrossoverLength);
            int.TryParse(textBox1.Text, out shelfCount);
            decimal.TryParse(textBox2.Text, out warehouseHeight);

            if (blocks <= 0) blocks = 2;
            if (aisles <= 0) aisles = 3;
            if (locations <= 0) locations = 5;
            if (shelfCount <= 0) shelfCount = 1;
            selectedShelf = Math.Min(Math.Max(selectedShelf, 1), shelfCount);
            PopulateShelfSelector();

            rotaYolu.Clear();
            routeResults.Clear();
            manualRouteNumber = 1;
            UpdateResultsPanel();

            BuildLayout();

            layoutHazir = true;

            panelWarehouse.Invalidate();
        }

        // ==========================================
        // LAYOUT OLUÅTUR
        // ==========================================
        private void BuildLayout()
        {
            hucreler.Clear();
            crossoverNoktalari.Clear();

            List<int> aisleXCoords = new List<int>();
            List<int> crossAisleYCoords = new List<int>();

            for (int i = 0; i < aisles; i++)
            {
                int x = StartX + i * (CorridorW + 2 * CellW);
                aisleXCoords.Add(x);
            }

            for (int j = 0; j <= blocks; j++)
            {
                int y = StartY + j * (locations * CellH + CrossAisleH);
                crossAisleYCoords.Add(y);
            }

            foreach (int x in aisleXCoords)
            {
                foreach (int y in crossAisleYCoords)
                {
                    crossoverNoktalari.Add(new CrossoverPoint
                    {
                        Point = new Point(x, y),
                        IsVisited = false
                    });
                }
            }

            int index = 0;

            for (int b = 0; b < blocks; b++)
            {
                int blockTopY = crossAisleYCoords[b] + CrossAisleH / 2;

                for (int a = 0; a <= aisles; a++)
                {
                    if (a == 0)
                    {
                        int leftRackX = aisleXCoords[0] - (CorridorW / 2) - CellW;

                        index = CreateRackGroup(
                            leftRackX,
                            blockTopY,
                            1,
                            index
                        );
                    }
                    else if (a == aisles)
                    {
                        int rightRackX = aisleXCoords[a - 1] + (CorridorW / 2);

                        index = CreateRackGroup(
                            rightRackX,
                            blockTopY,
                            1,
                            index
                        );
                    }
                    else
                    {
                        int doubleRackX = aisleXCoords[a - 1] + (CorridorW / 2);

                        index = CreateRackGroup(
                            doubleRackX,
                            blockTopY,
                            2,
                            index
                        );
                    }
                }
            }

            int leftWall =
                aisleXCoords.First() - CorridorW / 2 - CellW - 20;

            int rightWall =
                aisleXCoords.Last() + CorridorW / 2 + CellW + 20;

            int topWall =
                crossAisleYCoords.First() - CrossAisleH / 2 - 20;

            int bottomWall =
                crossAisleYCoords.Last() + CrossAisleH / 2 + 20;

            warehouseBounds = new Rectangle(
                leftWall,
                topWall,
                rightWall - leftWall,
                bottomWall - topWall
            );

            depotRect = new RectangleF(
                warehouseBounds.Left + 20,
                warehouseBounds.Bottom,
                60,
                26
            );
        }

        // ==========================================
        // RAF OLUÅTUR
        // ==========================================
        private int CreateRackGroup(
            int startX,
            int startY,
            int columns,
            int startIndex)
        {
            for (int c = 0; c < columns; c++)
            {
                int x = startX + c * CellW;

                for (int l = 0; l < locations; l++)
                {
                    int y = startY + l * CellH;

                    hucreler.Add(new RackCell
                    {
                        Bounds = new Rectangle(x, y, CellW, CellH),
                        Index = startIndex++,
                        IsSelected = false,
                        IsPicked = false
                    });
                }
            }

            return startIndex;
        }

        // ==========================================
        // PATH EFFECT
        // ==========================================
        private void ProcessPathSegment(Point p1, Point p2)
        {
            foreach (var cp in crossoverNoktalari)
            {
                if (IsPointNearSegment(cp.Point, p1, p2, 12))
                {
                    cp.IsVisited = true;
                }
            }

            foreach (var cell in hucreler)
            {
                if (cell.IsSelected)
                {
                    if (IsCellPickedByAisleSegment(cell, p1, p2))
                    {
                        foreach (int shelf in cell.SelectedShelves)
                            cell.PickedShelves.Add(shelf);

                        cell.IsPicked = true;
                    }
                }
            }
        }

        private bool IsCellPickedByAisleSegment(RackCell cell, Point p1, Point p2)
        {
            if (p1.X != p2.X)
                return false;

            var aisleXs = crossoverNoktalari
                .Select(c => c.Point.X)
                .Distinct()
                .ToList();

            if (aisleXs.Count == 0)
                return false;

            int cellCenterX = cell.Bounds.X + CellW / 2;
            int cellCenterY = cell.Bounds.Y + CellH / 2;
            int nearestAisleX = aisleXs
                .OrderBy(x => Math.Abs(x - cellCenterX))
                .First();

            if (Math.Abs(p1.X - nearestAisleX) > CorridorW / 2)
                return false;

            int minY = Math.Min(p1.Y, p2.Y);
            int maxY = Math.Max(p1.Y, p2.Y);

            return cellCenterY >= minY - 2 && cellCenterY <= maxY + 2;
        }

        // ==========================================
        // MESAFE HESABI
        // ==========================================
        private bool IsPointNearSegment(
            Point p,
            Point s1,
            Point s2,
            double tolerance)
        {
            double dx = s2.X - s1.X;
            double dy = s2.Y - s1.Y;

            if (dx == 0 && dy == 0)
            {
                return Math.Sqrt(
                    (p.X - s1.X) * (p.X - s1.X) +
                    (p.Y - s1.Y) * (p.Y - s1.Y)
                ) <= tolerance;
            }

            double t =
                ((p.X - s1.X) * dx + (p.Y - s1.Y) * dy) /
                (dx * dx + dy * dy);

            if (t < 0) t = 0;
            if (t > 1) t = 1;

            double closestX = s1.X + t * dx;
            double closestY = s1.Y + t * dy;

            double dist = Math.Sqrt(
                (p.X - closestX) * (p.X - closestX) +
                (p.Y - closestY) * (p.Y - closestY)
            );

            return dist <= tolerance;
        }

        // ==========================================
        // PAINT
        // ==========================================
        private void panelWarehouse_Paint(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;

            g.SmoothingMode =
                System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

            g.Clear(Color.White);

            if (!layoutHazir)
                return;

            DrawWarehouseBorder(g);
            DrawRacks(g);

            DrawRoute(g);

            DrawActiveCrossovers(g);
            DrawDepotBox(g);
            DrawLegend(g);
        }

        // ==========================================
        // RAF Ã‡Ä°Z
        // ==========================================
        private void DrawRacks(Graphics g)
        {
            using (Pen rackPen = new Pen(Color.Black, 1))
            {
                foreach (var cell in hucreler)
                {
                    if (cell.PickedShelves.Contains(selectedShelf))
                        g.FillRectangle(Brushes.Black, cell.Bounds);
                    else if (cell.SelectedShelves.Contains(selectedShelf))
                        g.FillRectangle(Brushes.Blue, cell.Bounds);
                    else
                        g.FillRectangle(Brushes.White, cell.Bounds);

                    g.DrawRectangle(rackPen, cell.Bounds);
                }
            }
        }

        // ==========================================
        // HASHTAG Ã‡Ä°Z
        // ==========================================
        private void DrawActiveCrossovers(Graphics g)
        {
            using (Pen redPen = new Pen(Color.Red, 2))
            {
                int offsetW = CorridorW / 4;
                int offsetH = CrossAisleH / 4;
                int lineLenW = CorridorW / 2;
                int lineLenH = CrossAisleH / 2;

                foreach (var cp in crossoverNoktalari)
                {
                    if (!cp.IsVisited)
                        continue;

                    Point p = cp.Point;

                    g.DrawLine(
                        redPen,
                        p.X - lineLenW,
                        p.Y - offsetH,
                        p.X + lineLenW,
                        p.Y - offsetH
                    );

                    g.DrawLine(
                        redPen,
                        p.X - lineLenW,
                        p.Y + offsetH,
                        p.X + lineLenW,
                        p.Y + offsetH
                    );

                    g.DrawLine(
                        redPen,
                        p.X - offsetW,
                        p.Y - lineLenH,
                        p.X - offsetW,
                        p.Y + lineLenH
                    );

                    g.DrawLine(
                        redPen,
                        p.X + offsetW,
                        p.Y - lineLenH,
                        p.X + offsetW,
                        p.Y + lineLenH
                    );
                }
            }
        }

        // ==========================================
        // BORDER Ã‡Ä°Z
        // ==========================================
        private void DrawWarehouseBorder(Graphics g)
        {
            using (Pen borderPen = new Pen(Color.Black, 2))
            {
                g.DrawRectangle(borderPen, warehouseBounds);
            }
        }

        // ==========================================
        // DEPOT Ã‡Ä°Z
        // ==========================================
        private void DrawDepotBox(Graphics g)
        {
            g.FillRectangle(
                Brushes.White,
                Rectangle.Round(depotRect)
            );

            g.DrawRectangle(
                Pens.Black,
                Rectangle.Round(depotRect)
            );

            g.DrawString(
                "depot",
                this.Font,
                Brushes.Black,
                depotRect.X + 4,
                depotRect.Y + 5
            );
        }

        private void DrawLegend(Graphics g)
        {
            int x = warehouseBounds.Left;
            int y = warehouseBounds.Top - 34;

            using (Font legendFont = new Font("Segoe UI", 8F))
            {
                DrawLegendItem(g, x, y, Color.White, "empty", legendFont);
                DrawLegendItem(g, x + 90, y, Color.FromArgb(25, 118, 210), "order", legendFont);
                DrawLegendItem(g, x + 180, y, Color.Black, "picked", legendFont);
                DrawLegendItem(g, x + 270, y, Color.Red, "route", legendFont);
            }
        }

        private void DrawRoute(Graphics g)
        {
            if (rotaYolu.Count <= 1)
                return;

            DrawSegmentedRoute(g);
        }

        private void DrawSegmentedRoute(Graphics g)
        {
            List<RouteDrawSegment> drawSegments = BuildRouteDrawSegments();
            Dictionary<string, int> segmentTotals = new Dictionary<string, int>();
            Dictionary<string, int> segmentOccurrences = new Dictionary<string, int>();
            stableSegmentOffsets.Clear();

            foreach (var segment in drawSegments)
            {
                if (!segmentTotals.ContainsKey(segment.Key))
                    segmentTotals[segment.Key] = 0;

                segmentTotals[segment.Key]++;
            }

            using (Pen routePen = new Pen(Color.Red, 2))
            {
                routePen.StartCap = System.Drawing.Drawing2D.LineCap.Round;
                routePen.EndCap = System.Drawing.Drawing2D.LineCap.Round;
                routePen.LineJoin = System.Drawing.Drawing2D.LineJoin.Round;

                List<Point> shiftedStarts = new List<Point>();
                List<Point> shiftedEnds = new List<Point>();

                foreach (var segment in drawSegments)
                {
                    if (!segmentOccurrences.ContainsKey(segment.Key))
                        segmentOccurrences[segment.Key] = 0;

                    segmentOccurrences[segment.Key]++;

                    int offset = GetRepeatedSegmentOffset(
                        segmentTotals[segment.Key],
                        segmentOccurrences[segment.Key]
                    );

                    stableSegmentOffsets[segment.Key] = offset;

                    Point shiftedP1 = OffsetRoutePoint(segment.From, segment.To, offset);
                    Point shiftedP2 = OffsetRoutePoint(segment.To, segment.From, offset);

                    shiftedStarts.Add(shiftedP1);
                    shiftedEnds.Add(shiftedP2);
                    g.DrawLine(routePen, shiftedP1, shiftedP2);
                }
            }
        }

        private List<RouteDrawSegment> BuildRouteDrawSegments()
        {
            List<RouteDrawSegment> result = new List<RouteDrawSegment>();

            for (int i = 0; i < rotaYolu.Count - 1; i++)
            {
                Point p1 = rotaYolu[i];
                Point p2 = rotaYolu[i + 1];

                foreach (var segment in SplitRouteSegment(p1, p2))
                    result.Add(segment);
            }

            return result;
        }

        private List<RouteDrawSegment> SplitRouteSegment(Point p1, Point p2)
        {
            List<RouteDrawSegment> result = new List<RouteDrawSegment>();

            if (p1 == p2)
                return result;

            List<int> splitValues = new List<int>();

            if (p1.X == p2.X)
            {
                int splitX = p1.X;
                List<int> aisleXs = GetAisleXs();

                if (aisleXs.Count > 0)
                {
                    int nearestX = GetNearestValue(aisleXs, p1.X);
                    if (Math.Abs(nearestX - p1.X) <= CorridorW / 2)
                        splitX = nearestX;
                }

                splitValues.Add(p1.Y);
                splitValues.Add(p2.Y);
                splitValues.AddRange(GetVerticalSplitValues(splitX));
                splitValues = splitValues
                    .Where(y => y >= Math.Min(p1.Y, p2.Y) && y <= Math.Max(p1.Y, p2.Y))
                    .Distinct()
                    .OrderBy(y => y)
                    .ToList();

                if (p2.Y < p1.Y)
                    splitValues.Reverse();

                for (int i = 0; i < splitValues.Count - 1; i++)
                {
                    Point from = new Point(splitX, splitValues[i]);
                    Point to = new Point(splitX, splitValues[i + 1]);
                    AddDrawSegment(result, from, to);
                }

                return result;
            }

            if (p1.Y == p2.Y)
            {
                int splitY = p1.Y;
                List<int> crossYs = GetCrossAisleYs();

                if (crossYs.Count > 0)
                {
                    int nearestY = GetNearestValue(crossYs, p1.Y);
                    if (Math.Abs(nearestY - p1.Y) <= CrossAisleH / 2)
                        splitY = nearestY;
                }

                splitValues.Add(p1.X);
                splitValues.Add(p2.X);
                splitValues.AddRange(GetHorizontalSplitValues(splitY));
                splitValues = splitValues
                    .Where(x => x >= Math.Min(p1.X, p2.X) && x <= Math.Max(p1.X, p2.X))
                    .Distinct()
                    .OrderBy(x => x)
                    .ToList();

                if (p2.X < p1.X)
                    splitValues.Reverse();

                for (int i = 0; i < splitValues.Count - 1; i++)
                {
                    Point from = new Point(splitValues[i], splitY);
                    Point to = new Point(splitValues[i + 1], splitY);
                    AddDrawSegment(result, from, to);
                }

                return result;
            }

            AddDrawSegment(result, p1, p2);
            return result;
        }

        private IEnumerable<int> GetVerticalSplitValues(int aisleX)
        {
            foreach (Point point in rotaYolu.Where(p => p.X == aisleX))
                yield return point.Y;

            foreach (Point point in crossoverNoktalari.Where(c => c.Point.X == aisleX).Select(c => c.Point))
                yield return point.Y;

            foreach (var cell in hucreler.Where(h => h.IsSelected || h.IsPicked))
            {
                int cellCenterX = cell.Bounds.X + CellW / 2;
                int nearestAisleX = crossoverNoktalari
                    .Select(c => c.Point.X)
                    .Distinct()
                    .OrderBy(x => Math.Abs(x - cellCenterX))
                    .FirstOrDefault();

                if (nearestAisleX == aisleX)
                    yield return cell.Bounds.Y + CellH / 2;
            }
        }

        private IEnumerable<int> GetHorizontalSplitValues(int crossY)
        {
            foreach (Point point in rotaYolu.Where(p => p.Y == crossY))
                yield return point.X;

            foreach (Point point in crossoverNoktalari.Where(c => c.Point.Y == crossY).Select(c => c.Point))
                yield return point.X;
        }

        private void AddDrawSegment(List<RouteDrawSegment> result, Point from, Point to)
        {
            if (from == to)
                return;

            NormalizeDrawSegment(ref from, ref to);

            if (from == to)
                return;

            result.Add(new RouteDrawSegment
            {
                From = from,
                To = to,
                Key = GetRouteSegmentKey(from, to)
            });
        }

        private void NormalizeDrawSegment(ref Point from, ref Point to)
        {
            List<int> aisleXs = GetAisleXs();
            List<int> crossYs = GetCrossAisleYs();

            if (from.X == to.X && aisleXs.Count > 0)
            {
                int nearestX = GetNearestValue(aisleXs, from.X);

                if (Math.Abs(nearestX - from.X) <= CorridorW / 2)
                {
                    from = new Point(nearestX, from.Y);
                    to = new Point(nearestX, to.Y);
                }

                return;
            }

            if (from.Y == to.Y && crossYs.Count > 0)
            {
                int nearestY = GetNearestValue(crossYs, from.Y);

                if (Math.Abs(nearestY - from.Y) <= CrossAisleH / 2)
                {
                    from = new Point(from.X, nearestY);
                    to = new Point(to.X, nearestY);
                }
            }
        }

        private string GetRouteSegmentKey(Point p1, Point p2)
        {
            if (p1.X == p2.X)
            {
                int minY = Math.Min(p1.Y, p2.Y);
                int maxY = Math.Max(p1.Y, p2.Y);
                return "V:" + p1.X + ":" + minY + ":" + maxY;
            }

            if (p1.Y == p2.Y)
            {
                int minX = Math.Min(p1.X, p2.X);
                int maxX = Math.Max(p1.X, p2.X);
                return "H:" + p1.Y + ":" + minX + ":" + maxX;
            }

            int minXDiag = Math.Min(p1.X, p2.X);
            int minYDiag = Math.Min(p1.Y, p2.Y);
            int maxXDiag = Math.Max(p1.X, p2.X);
            int maxYDiag = Math.Max(p1.Y, p2.Y);
            return "D:" + minXDiag + ":" + minYDiag + ":" + maxXDiag + ":" + maxYDiag;
        }

        private int GetRepeatedSegmentOffset(int totalPasses, int occurrence)
        {
            if (totalPasses <= 1)
                return 0;

            if (totalPasses == 2)
                return occurrence == 1 ? 4 : -4;

            if (occurrence == 1)
                return 6;

            if (occurrence == 2)
                return 0;

            return -6;
        }

        private Point OffsetRoutePoint(Point point, Point otherPoint, int offset)
        {
            if (offset == 0)
                return point;

            if (point.X == otherPoint.X)
                return new Point(point.X + offset, point.Y);

            if (point.Y == otherPoint.Y)
                return new Point(point.X, point.Y + offset);

            return point;
        }

        private List<int> GetAisleXs()
        {
            return crossoverNoktalari
                .Select(c => c.Point.X)
                .Distinct()
                .OrderBy(x => x)
                .ToList();
        }

        private List<int> GetCrossAisleYs()
        {
            return crossoverNoktalari
                .Select(c => c.Point.Y)
                .Distinct()
                .OrderBy(y => y)
                .ToList();
        }

        private int GetNearestValue(List<int> values, int value)
        {
            return values.OrderBy(v => Math.Abs(v - value)).First();
        }

        private bool IsCloseToAny(List<int> values, int value, int tolerance)
        {
            return values.Any(v => Math.Abs(v - value) <= tolerance);
        }

        private bool IsPointOnSegment(Point point, Point s1, Point s2)
        {
            if (s1.X == s2.X)
            {
                return point.X == s1.X &&
                    point.Y >= Math.Min(s1.Y, s2.Y) &&
                    point.Y <= Math.Max(s1.Y, s2.Y);
            }

            if (s1.Y == s2.Y)
            {
                return point.Y == s1.Y &&
                    point.X >= Math.Min(s1.X, s2.X) &&
                    point.X <= Math.Max(s1.X, s2.X);
            }

            return false;
        }

        private int ManhattanDistance(Point p1, Point p2)
        {
            return Math.Abs(p1.X - p2.X) + Math.Abs(p1.Y - p2.Y);
        }

        private bool SegmentPicksAnySelectedCell(Point p1, Point p2)
        {
            return hucreler.Any(h => h.IsSelected && IsCellPickedByAisleSegment(h, p1, p2));
        }

        private int GetSelectedOrderCount()
        {
            return hucreler.Sum(h => h.SelectedShelves.Count);
        }

        private int GetPickedOrderCount()
        {
            return hucreler.Sum(h => h.PickedShelves.Count);
        }

        private void ClearAllOrders()
        {
            foreach (var h in hucreler)
            {
                h.SelectedShelves.Clear();
                h.PickedShelves.Clear();
                h.IsSelected = false;
                h.IsPicked = false;
            }
        }

        private void ClearPickedOrders()
        {
            foreach (var h in hucreler)
            {
                h.PickedShelves.Clear();
                h.IsPicked = false;
            }
        }

        private bool SegmentVisitsAnyCrossover(Point p1, Point p2)
        {
            return crossoverNoktalari.Any(cp => IsPointOnSegment(cp.Point, p1, p2));
        }

        private bool SegmentShouldBeKeptOnManualReturn(Point p1, Point p2)
        {
            return SegmentPicksAnySelectedCell(p1, p2);
        }

        private void RecalculateRouteEffects()
        {
            foreach (var cp in crossoverNoktalari)
                cp.IsVisited = false;

            ClearPickedOrders();

            for (int i = 0; i < rotaYolu.Count - 1; i++)
                ProcessPathSegment(rotaYolu[i], rotaYolu[i + 1]);
        }

        private void DrawLegendItem(Graphics g, int x, int y, Color color, string text, Font font)
        {
            using (Brush brush = new SolidBrush(color))
            using (Pen pen = new Pen(Color.FromArgb(65, 75, 85)))
            {
                g.FillRectangle(brush, x, y + 2, 14, 14);
                g.DrawRectangle(pen, x, y + 2, 14, 14);
                g.DrawString(text, font, Brushes.Black, x + 20, y);
            }
        }

        // ==========================================
        // RANDOM ORDER
        // ==========================================
        private void button2_Click(object sender, EventArgs e)
        {
            if (!layoutHazir)
                return;

            if (!int.TryParse(textBox7.Text, out int adet))
                return;

            rotaYolu.Clear();
            routeResults.Clear();
            manualRouteNumber = 1;
            UpdateResultsPanel();

            foreach (var h in hucreler)
            {
                h.IsSelected = false;
                h.IsPicked = false;
                h.SelectedShelves.Clear();
                h.PickedShelves.Clear();
            }

            foreach (var cp in crossoverNoktalari)
            {
                cp.IsVisited = false;
            }

            List<int> indexler =
                Enumerable.Range(0, hucreler.Count).ToList();

            for (int i = 0; i < adet && indexler.Count > 0; i++)
            {
                int r = rnd.Next(indexler.Count);

                int idx = indexler[r];

                hucreler[idx].IsSelected = true;
                hucreler[idx].SelectedShelves.Add(rnd.Next(1, shelfCount + 1));

                indexler.RemoveAt(r);
            }

            panelWarehouse.Invalidate();
        }

        private void StartManualRoute()
        {
            if (!layoutHazir)
                return;

            cizimModu = true;
            rotaYolu.Clear();

            foreach (var cp in crossoverNoktalari)
                cp.IsVisited = false;

            ClearPickedOrders();

            Point depotMerkez = new Point(
                (int)(depotRect.X + depotRect.Width / 2),
                (int)(depotRect.Y)
            );

            rotaYolu.Add(depotMerkez);
            sonMouseNoktasi = depotMerkez;

            panelWarehouse.Invalidate();
        }

        // ==========================================
        // TEMÄ°ZLE
        // ==========================================
        private void button3_Click(object sender, EventArgs e)
        {
            rotaYolu.Clear();

            foreach (var h in hucreler)
            {
                h.IsSelected = false;
                h.IsPicked = false;
                h.SelectedShelves.Clear();
                h.PickedShelves.Clear();
            }

            foreach (var cp in crossoverNoktalari)
            {
                cp.IsVisited = false;
            }

            panelWarehouse.Invalidate();
        }

        // ==========================================
        // OPTIMAL ROTA (RATLIFF-ROSENTHAL)
        // ==========================================
        private void button5_Click(object sender, EventArgs e)
        {
            if (!layoutHazir)
                return;

            rotaYolu.Clear();
            foreach (var cp in crossoverNoktalari) cp.IsVisited = false;
            ClearPickedOrders();
            cizimModu = false;

            var aisleXs = crossoverNoktalari
                .Select(c => c.Point.X)
                .Distinct()
                .OrderBy(x => x)
                .ToList();

            var crossYs = crossoverNoktalari
                .Select(c => c.Point.Y)
                .Distinct()
                .OrderBy(y => y)
                .ToList();

            if (aisleXs.Count == 0 || crossYs.Count == 0)
                return;

            if (crossYs.Count == 2)
            {
                RunRatliffRosenthalRoute(aisleXs, crossYs);
                return;
            }

            RunMetricClosureOptimalRoute(aisleXs, crossYs);
        }

        private void RunRatliffRosenthalRoute(List<int> aisleXs, List<int> crossYs)
        {
            RatliffRosenthalCandidate candidate = BuildRatliffRosenthalRoute(aisleXs, crossYs);

            if (candidate == null || candidate.Route.Count < 2)
            {
                panelWarehouse.Invalidate();
                return;
            }

            rotaYolu = SimplifyRoute(candidate.Route);

            for (int i = 0; i < rotaYolu.Count - 1; i++)
            {
                ProcessPathSegment(rotaYolu[i], rotaYolu[i + 1]);
            }

            MarkSelectedCellsPicked();

            double optimalDistance = CalculateRouteDistance();
            routeResults["Optimal"] = optimalDistance;
            StatsService.Record(loggedInUser, "Optimal", Math.Max(GetPickedOrderCount(), GetSelectedOrderCount()), GetSelectedVehicle(), (decimal)optimalDistance);
            UpdateResultsPanel();
            panelWarehouse.Invalidate();
        }

        private void RunMetricClosureOptimalRoute(List<int> aisleXs, List<int> crossYs)
        {
            List<RouteNode> nodes = BuildOptimalRouteNodes(aisleXs);

            if (nodes.Count <= 1)
            {
                panelWarehouse.Invalidate();
                return;
            }

            int itemCount = nodes.Count - 1;

            if (itemCount > 18)
            {
                MessageBox.Show(
                    "Optimal hesaplama en fazla 18 urun icin yapilir. Daha fazla urun icin heuristic yontemleri kullanin.",
                    "Optimal rota",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information
                );
                return;
            }

            double[,] dist = new double[nodes.Count, nodes.Count];

            for (int i = 0; i < nodes.Count; i++)
            {
                for (int j = 0; j < nodes.Count; j++)
                {
                    dist[i, j] = GetNetworkDistance(nodes[i].Point, nodes[j].Point, crossYs);
                }
            }

            List<int> visitOrder = SolveShortestClosedTour(dist);
            List<Point> finalRoute = new List<Point>();

            for (int i = 0; i < visitOrder.Count - 1; i++)
            {
                List<Point> leg = BuildShortestNetworkPath(
                    nodes[visitOrder[i]].Point,
                    nodes[visitOrder[i + 1]].Point,
                    crossYs
                );

                AppendRouteLeg(finalRoute, leg);
            }

            rotaYolu = SimplifyRoute(finalRoute);

            for (int i = 0; i < rotaYolu.Count - 1; i++)
            {
                ProcessPathSegment(rotaYolu[i], rotaYolu[i + 1]);
            }

            foreach (int nodeIndex in visitOrder)
            {
                if (nodeIndex > 0 && nodes[nodeIndex].Cell != null)
                {
                    foreach (int shelf in nodes[nodeIndex].Cell.SelectedShelves)
                        nodes[nodeIndex].Cell.PickedShelves.Add(shelf);

                    nodes[nodeIndex].Cell.IsPicked = true;
                }
            }

            double optimalDistance = CalculateRouteDistance();
            routeResults["Optimal"] = optimalDistance;
            StatsService.Record(loggedInUser, "Optimal", Math.Max(GetPickedOrderCount(), GetSelectedOrderCount()), GetSelectedVehicle(), (decimal)optimalDistance);
            UpdateResultsPanel();
            panelWarehouse.Invalidate();
        }

        private RatliffRosenthalCandidate BuildRatliffRosenthalRoute(List<int> aisleXs, List<int> crossYs)
        {
            int topY = crossYs.First();
            int bottomY = crossYs.Last();
            Point depotPoint = new Point((int)(depotRect.X + depotRect.Width / 2), (int)(depotRect.Y));
            Point depotOnFront = new Point(depotPoint.X, bottomY);
            List<RatliffRosenthalAisle> aislesData = BuildRatliffRosenthalAisles(aisleXs, topY, bottomY);

            if (!aislesData.Any(a => a.HasItems))
                return null;

            RatliffRosenthalCandidate best = BuildFrontOnlyRatliffRosenthalCandidate(aislesData, depotPoint, depotOnFront, bottomY);

            for (int up = 0; up < aislesData.Count; up++)
            {
                for (int down = up; down < aislesData.Count; down++)
                {
                    RatliffRosenthalCandidate candidate = BuildLoopRatliffRosenthalCandidate(
                        aislesData,
                        up,
                        down,
                        depotPoint,
                        depotOnFront,
                        topY,
                        bottomY
                    );

                    if (best == null || candidate.Cost < best.Cost)
                        best = candidate;
                }
            }

            return best;
        }

        private List<RatliffRosenthalAisle> BuildRatliffRosenthalAisles(List<int> aisleXs, int topY, int bottomY)
        {
            Dictionary<int, List<Rectangle>> aisleItems = GetSelectedItemsByNearestAisle(aisleXs);
            List<RatliffRosenthalAisle> result = new List<RatliffRosenthalAisle>();

            foreach (int aisleX in aisleXs)
            {
                RatliffRosenthalAisle aisle = new RatliffRosenthalAisle
                {
                    X = aisleX,
                    HasItems = aisleItems.ContainsKey(aisleX)
                };

                if (aisle.HasItems)
                {
                    List<int> itemYs = aisleItems[aisleX]
                        .Select(r => r.Y + CellH / 2)
                        .OrderBy(y => y)
                        .ToList();

                    aisle.TopMostItemY = itemYs.First();
                    aisle.BottomMostItemY = itemYs.Last();

                    int largestGapStart = topY;
                    int largestGapEnd = itemYs.First();
                    int largestGap = largestGapEnd - largestGapStart;

                    for (int i = 0; i < itemYs.Count - 1; i++)
                    {
                        int gap = itemYs[i + 1] - itemYs[i];
                        if (gap > largestGap)
                        {
                            largestGap = gap;
                            largestGapStart = itemYs[i];
                            largestGapEnd = itemYs[i + 1];
                        }
                    }

                    int bottomGap = bottomY - itemYs.Last();
                    if (bottomGap > largestGap)
                    {
                        largestGap = bottomGap;
                        largestGapStart = itemYs.Last();
                        largestGapEnd = bottomY;
                    }

                    aisle.TopLoopEndY = largestGapStart;
                    aisle.BottomLoopEndY = largestGapEnd;
                }

                result.Add(aisle);
            }

            return result;
        }

        private RatliffRosenthalCandidate BuildFrontOnlyRatliffRosenthalCandidate(
            List<RatliffRosenthalAisle> aislesData,
            Point depotPoint,
            Point depotOnFront,
            int bottomY)
        {
            List<RatliffRosenthalAisle> aislesWithItems = aislesData.Where(a => a.HasItems).ToList();
            List<Point> route = new List<Point>();
            route.Add(depotPoint);
            route.Add(depotOnFront);

            foreach (RatliffRosenthalAisle aisle in aislesWithItems.OrderBy(a => Math.Abs(a.X - depotPoint.X)))
            {
                route.Add(new Point(aisle.X, bottomY));
                route.Add(new Point(aisle.X, aisle.TopMostItemY));
                route.Add(new Point(aisle.X, bottomY));
            }

            route.Add(depotOnFront);
            route.Add(depotPoint);

            return new RatliffRosenthalCandidate
            {
                Cost = CalculateRawRouteDistance(route),
                Route = route
            };
        }

        private RatliffRosenthalCandidate BuildLoopRatliffRosenthalCandidate(
            List<RatliffRosenthalAisle> aislesData,
            int upIndex,
            int downIndex,
            Point depotPoint,
            Point depotOnFront,
            int topY,
            int bottomY)
        {
            RatliffRosenthalAisle upAisle = aislesData[upIndex];
            RatliffRosenthalAisle downAisle = aislesData[downIndex];
            List<Point> route = new List<Point>();
            route.Add(depotPoint);
            route.Add(depotOnFront);
            route.Add(new Point(upAisle.X, bottomY));
            route.Add(new Point(upAisle.X, topY));

            for (int i = upIndex + 1; i < downIndex; i++)
            {
                RatliffRosenthalAisle aisle = aislesData[i];
                route.Add(new Point(aisle.X, topY));

                if (aisle.HasItems)
                {
                    route.Add(new Point(aisle.X, aisle.TopLoopEndY));
                    route.Add(new Point(aisle.X, topY));
                }
            }

            route.Add(new Point(downAisle.X, topY));
            route.Add(new Point(downAisle.X, bottomY));

            foreach (RatliffRosenthalAisle aisle in aislesData.Skip(downIndex + 1).Where(a => a.HasItems).OrderBy(a => a.X))
            {
                route.Add(new Point(aisle.X, bottomY));
                route.Add(new Point(aisle.X, aisle.TopMostItemY));
                route.Add(new Point(aisle.X, bottomY));
            }

            for (int i = downIndex - 1; i > upIndex; i--)
            {
                RatliffRosenthalAisle aisle = aislesData[i];
                route.Add(new Point(aisle.X, bottomY));

                if (aisle.HasItems)
                {
                    route.Add(new Point(aisle.X, aisle.BottomLoopEndY));
                    route.Add(new Point(aisle.X, bottomY));
                }
            }

            foreach (RatliffRosenthalAisle aisle in aislesData.Take(upIndex).Where(a => a.HasItems).OrderByDescending(a => a.X))
            {
                route.Add(new Point(aisle.X, bottomY));
                route.Add(new Point(aisle.X, aisle.TopMostItemY));
                route.Add(new Point(aisle.X, bottomY));
            }

            route.Add(depotOnFront);
            route.Add(depotPoint);

            return new RatliffRosenthalCandidate
            {
                Cost = CalculateRawRouteDistance(route),
                Route = route
            };
        }

        private double CalculateRawRouteDistance(List<Point> route)
        {
            double distance = 0;

            for (int i = 0; i < route.Count - 1; i++)
                distance += ManhattanDistance(route[i], route[i + 1]);

            return distance;
        }

        private void MarkSelectedCellsPicked()
        {
            foreach (var cell in hucreler.Where(h => h.IsSelected))
            {
                foreach (int shelf in cell.SelectedShelves)
                    cell.PickedShelves.Add(shelf);

                cell.IsPicked = true;
            }
        }

        private List<RouteNode> BuildOptimalRouteNodes(List<int> aisleXs)
        {
            List<RouteNode> nodes = new List<RouteNode>();
            Point depotPoint = new Point(
                (int)(depotRect.X + depotRect.Width / 2),
                (int)(depotRect.Y)
            );

            nodes.Add(new RouteNode
            {
                Point = depotPoint,
                IsDepot = true,
                Cell = null
            });

            foreach (var cell in hucreler.Where(h => h.IsSelected))
            {
                int cellCenterX = cell.Bounds.X + CellW / 2;
                int nearestAisleX = aisleXs
                    .OrderBy(x => Math.Abs(x - cellCenterX))
                    .First();

                nodes.Add(new RouteNode
                {
                    Point = new Point(nearestAisleX, cell.Bounds.Y + CellH / 2),
                    IsDepot = false,
                    Cell = cell
                });
            }

            return nodes;
        }

        private double GetNetworkDistance(Point p1, Point p2, List<int> crossYs)
        {
            if (p1.X == p2.X)
                return Math.Abs(p1.Y - p2.Y);

            return crossYs.Min(y =>
                Math.Abs(p1.Y - y) +
                Math.Abs(p1.X - p2.X) +
                Math.Abs(p2.Y - y)
            );
        }

        private List<Point> BuildShortestNetworkPath(Point p1, Point p2, List<int> crossYs)
        {
            if (p1.X == p2.X)
                return new List<Point> { p1, p2 };

            int bestY = crossYs
                .OrderBy(y =>
                    Math.Abs(p1.Y - y) +
                    Math.Abs(p1.X - p2.X) +
                    Math.Abs(p2.Y - y))
                .First();

            return new List<Point>
            {
                p1,
                new Point(p1.X, bestY),
                new Point(p2.X, bestY),
                p2
            };
        }

        private List<int> SolveShortestClosedTour(double[,] dist)
        {
            int n = dist.GetLength(0);
            int itemCount = n - 1;
            int fullMask = (1 << itemCount) - 1;
            const double Inf = double.MaxValue / 4;

            double[,] dp = new double[1 << itemCount, n];
            int[,] parent = new int[1 << itemCount, n];

            for (int mask = 0; mask <= fullMask; mask++)
            {
                for (int i = 0; i < n; i++)
                {
                    dp[mask, i] = Inf;
                    parent[mask, i] = -1;
                }
            }

            for (int item = 1; item < n; item++)
            {
                int bit = 1 << (item - 1);
                dp[bit, item] = dist[0, item];
                parent[bit, item] = 0;
            }

            for (int mask = 1; mask <= fullMask; mask++)
            {
                for (int last = 1; last < n; last++)
                {
                    if ((mask & (1 << (last - 1))) == 0)
                        continue;

                    double current = dp[mask, last];
                    if (current >= Inf)
                        continue;

                    for (int next = 1; next < n; next++)
                    {
                        int nextBit = 1 << (next - 1);
                        if ((mask & nextBit) != 0)
                            continue;

                        int nextMask = mask | nextBit;
                        double candidate = current + dist[last, next];

                        if (candidate < dp[nextMask, next])
                        {
                            dp[nextMask, next] = candidate;
                            parent[nextMask, next] = last;
                        }
                    }
                }
            }

            double best = Inf;
            int bestLast = 0;

            for (int last = 1; last < n; last++)
            {
                double candidate = dp[fullMask, last] + dist[last, 0];

                if (candidate < best)
                {
                    best = candidate;
                    bestLast = last;
                }
            }

            List<int> reversed = new List<int>();
            int currentMask = fullMask;
            int currentLast = bestLast;

            while (currentLast != 0 && currentLast != -1)
            {
                reversed.Add(currentLast);
                int prev = parent[currentMask, currentLast];
                currentMask &= ~(1 << (currentLast - 1));
                currentLast = prev;
            }

            reversed.Reverse();

            List<int> order = new List<int>();
            order.Add(0);
            order.AddRange(reversed);
            order.Add(0);

            return order;
        }

        private void AppendRouteLeg(List<Point> route, List<Point> leg)
        {
            foreach (Point point in leg)
            {
                if (route.Count > 0 && route[route.Count - 1] == point)
                    continue;

                route.Add(point);
            }
        }

        private List<Point> SimplifyRoute(List<Point> route)
        {
            if (route.Count <= 2)
                return route;

            List<Point> simplified = new List<Point>();
            simplified.Add(route[0]);

            for (int i = 1; i < route.Count - 1; i++)
            {
                Point previous = simplified[simplified.Count - 1];
                Point current = route[i];
                Point next = route[i + 1];

                bool sameVertical = previous.X == current.X && current.X == next.X;
                bool sameHorizontal = previous.Y == current.Y && current.Y == next.Y;
                bool verticalTurnBack =
                    sameVertical &&
                    Math.Sign(current.Y - previous.Y) != Math.Sign(next.Y - current.Y);
                bool horizontalTurnBack =
                    sameHorizontal &&
                    Math.Sign(current.X - previous.X) != Math.Sign(next.X - current.X);

                if ((!sameVertical && !sameHorizontal) || verticalTurnBack || horizontalTurnBack)
                    simplified.Add(current);
            }

            simplified.Add(route[route.Count - 1]);
            return simplified;
        }

        // ==========================================
        // YENÄ° ROTA (S-SHAPE ALGORÄ°TMASI)
        // ==========================================
        private void button6_Click(object sender, EventArgs e)
        {
            if (!layoutHazir)
                return;

            // 1. Ã–nceki yollarÄ± ve ziyaretleri temizle
            rotaYolu.Clear();
            foreach (var cp in crossoverNoktalari) cp.IsVisited = false;
            ClearPickedOrders();
            cizimModu = false;

            // 2. Hem dikey koridorlarÄ±n (X) hem de YATAY ARA KORÄ°DORLARIN (Y) konumlarÄ±nÄ± belirle
            var uniqueAisleXs = crossoverNoktalari.Select(c => c.Point.X).Distinct().OrderBy(x => x).ToList();
            var crossAisleYs = crossoverNoktalari.Select(c => c.Point.Y).Distinct().OrderBy(y => y).ToList(); // YukarÄ±dan aÅŸaÄŸÄ±ya sÄ±ralÄ±

            if (uniqueAisleXs.Count == 0 || crossAisleYs.Count == 0) return;

            int topY = crossAisleYs.First();
            int bottomY = crossAisleYs.Last();

            // 3. SADECE iÃ§erisinde "SeÃ§ili (BoyalÄ±) Kutu" olan gerÃ§ek koridorlarÄ± bul.
            // Her Ã¼rÃ¼n en yakÄ±n koridora baÄŸlanÄ±r; bÃ¶ylece komÅŸu koridor gereksiz yere rotaya girmez.
            Dictionary<int, List<Rectangle>> aisleItems = GetSelectedItemsByNearestAisle(uniqueAisleXs);
            List<int> aislesWithItems = aisleItems.Keys.OrderBy(x => x).ToList();

            // EÄŸer hiÃ§ Ã¼rÃ¼n yoksa Ã§Ä±kÄ±ÅŸ yap
            if (aislesWithItems.Count == 0)
            {
                panelWarehouse.Invalidate();
                return;
            }

            // 4. ROTAYI Ã‡Ä°Z (Depodan Ã‡Ä±kÄ±ÅŸ)
            Point depotMerkez = new Point((int)(depotRect.X + depotRect.Width / 2), (int)(depotRect.Y));
            rotaYolu.Add(depotMerkez);

            // Deponun Ã¶n koridoruna geÃ§ (alt cross-aisle)
            Point currentPos = new Point(depotMerkez.X, bottomY);
            rotaYolu.Add(currentPos);

            bool goingUp = true;

            // 5. Ara koridorlarÄ± kullanarak Dinamik S-Shape Ã‡iz
            for (int i = 0; i < aislesWithItems.Count; i++)
            {
                int currentAisleX = aislesWithItems[i];
                int nextAisleX = (i + 1 < aislesWithItems.Count) ? aislesWithItems[i + 1] : currentAisleX;

                // Hedef dikey koridora yatayda ilerle
                rotaYolu.Add(new Point(currentAisleX, currentPos.Y));
                currentPos = new Point(currentAisleX, currentPos.Y);

                if (goingUp)
                {
                    // --- YUKARI Ã‡IKARKEN ---
                    // Bu koridordaki en yÃ¼ksek Ã¼rÃ¼nÃ¼n Y deÄŸerini bul
                    int highestItemY = aisleItems[currentAisleX].Min(rect => rect.Top);

                    // Sonraki koridorun Ã¼rÃ¼nlerine geÃ§iÅŸ yapabilmek iÃ§in oradaki en yÃ¼ksek Ã¼rÃ¼nÃ¼ de hesaba kat
                    if (i < aislesWithItems.Count - 1)
                    {
                        int nextHighestItemY = aisleItems[nextAisleX].Min(rect => rect.Top);
                        highestItemY = Math.Min(highestItemY, nextHighestItemY);
                    }

                    // En tepedeki (topY) koridora gitmek yerine, Ã¼rÃ¼nÃ¼n hemen Ã¼stÃ¼ndeki Ä°LK ARA KORÄ°DORU bul
                    int targetY = topY;
                    var availableCrossAisles = crossAisleYs.Where(y => y <= highestItemY).ToList();
                    if (availableCrossAisles.Any())
                    {
                        targetY = availableCrossAisles.Last(); // ÃœrÃ¼nÃ¼n hizasÄ±nÄ± kurtaran en yakÄ±n Ã¼st koridor
                    }

                    rotaYolu.Add(new Point(currentAisleX, targetY));
                    currentPos = new Point(currentAisleX, targetY);
                }
                else
                {
                    // --- AÅAÄI Ä°NERKEN ---
                    // Bu koridordaki en alÃ§ak Ã¼rÃ¼nÃ¼n Y deÄŸerini bul
                    int lowestItemY = aisleItems[currentAisleX].Max(rect => rect.Bottom);

                    if (i < aislesWithItems.Count - 1)
                    {
                        int nextLowestItemY = aisleItems[nextAisleX].Max(rect => rect.Bottom);
                        lowestItemY = Math.Max(lowestItemY, nextLowestItemY);
                    }

                    // En alt (bottomY) koridora inmek yerine, Ã¼rÃ¼nÃ¼n hemen altÄ±ndaki Ä°LK ARA KORÄ°DORU bul
                    int targetY = bottomY;
                    var availableCrossAisles = crossAisleYs.Where(y => y >= lowestItemY).ToList();
                    if (availableCrossAisles.Any())
                    {
                        targetY = availableCrossAisles.First(); // ÃœrÃ¼nÃ¼n hizasÄ±nÄ± kurtaran en yakÄ±n alt koridor
                    }

                    rotaYolu.Add(new Point(currentAisleX, targetY));
                    currentPos = new Point(currentAisleX, targetY);
                }

                // YÃ¶nÃ¼ tersine Ã§evir
                goingUp = !goingUp;
            }

            // 6. DEPOYA DÃ–NÃœÅ Ä°ÅLEMÄ°
            int lastAisleX = aislesWithItems.Last();

            // EÄŸer algoritma ara bir koridorda bitirdiyse, depoya dÃ¶nmek iÃ§in Ã¶nce en alt koridora in
            if (currentPos.Y != bottomY)
            {
                rotaYolu.Add(new Point(lastAisleX, bottomY));
            }

            // Ã–n hattÄ± (Alt Cross-Aisle) takip ederek deponun merkezine geri dÃ¶n
            rotaYolu.Add(new Point(depotMerkez.X, bottomY));
            rotaYolu.Add(depotMerkez); // Depoya gir

            // 7. Yollar Ã¼zerindeki Crossover Hashtag'lerini ve Ä°ÅŸlenen KutularÄ± boya
            for (int i = 0; i < rotaYolu.Count - 1; i++)
            {
                ProcessPathSegment(rotaYolu[i], rotaYolu[i + 1]);
            }

            // EkranÄ± gÃ¼ncelle ve Ã§izimleri yansÄ±t
            panelWarehouse.Invalidate();
            StoreRouteResult("S-shape");
        }


        // ==========================================
        // COMBINED (BÄ°RLEÅÄ°K) ALGORÄ°TMASI
        // ==========================================
        private void button7_Click(object sender, EventArgs e)
        {
            if (!layoutHazir)
                return;

            // 1. Ã–nceki yollarÄ± ve ziyaret durumlarÄ±nÄ± temizle
            rotaYolu.Clear();
            foreach (var cp in crossoverNoktalari) cp.IsVisited = false;
            ClearPickedOrders();
            cizimModu = false;

            // 2. Koridor eksenlerini (X) ve yatay geÃ§iÅŸ yollarÄ±nÄ± (Y) getir
            var uniqueAisleXs = crossoverNoktalari.Select(c => c.Point.X).Distinct().OrderBy(x => x).ToList();
            var crossAisleYs = crossoverNoktalari.Select(c => c.Point.Y).Distinct().OrderBy(y => y).ToList();

            if (uniqueAisleXs.Count == 0 || crossAisleYs.Count == 0) return;

            int topY = crossAisleYs.First();
            int bottomY = crossAisleYs.Last();

            // 3. Ä°Ã§inde seÃ§ili Ã¼rÃ¼n olan gerÃ§ek koridorlarÄ± ve Ã¼rÃ¼nlerin sÄ±nÄ±r koordinatlarÄ±nÄ± bul
            Dictionary<int, List<Rectangle>> aisleItems = GetSelectedItemsByNearestAisle(uniqueAisleXs);
            List<int> aislesWithItems = aisleItems.Keys.OrderBy(x => x).ToList();

            if (aislesWithItems.Count == 0)
            {
                panelWarehouse.Invalidate();
                return;
            }

            // 4. DÄ°NAMÄ°K PROGRAMLAMA (DP) Ä°LE OPTÄ°MAL ROTAYI BULMA (Orijinal Site AlgoritmasÄ±)
            Point depotMerkez = new Point((int)(depotRect.X + depotRect.Width / 2), (int)(depotRect.Y));

            int N = aislesWithItems.Count;
            double[] costTop = new double[N];
            double[] costBot = new double[N];
            List<Point>[] pathTop = new List<Point>[N];
            List<Point>[] pathBot = new List<Point>[N];

            for (int i = 0; i < N; i++)
            {
                int currAisleX = aislesWithItems[i];
                var items = aisleItems[currAisleX];

                // ÃœrÃ¼nlerin gerÃ§ek sÄ±nÄ±r koordinatlarÄ±nÄ± alÄ±yoruz (Ã‡izginin tam Ã¼rÃ¼nde bitmesi iÃ§in)
                int minY = items.Min(r => r.Top);
                int maxY = items.Max(r => r.Bottom);

                // Bu koridor iÃ§in potansiyel eylem mesafeleri
                double L = Math.Abs(bottomY - topY);
                double distReturnTop = 2.0 * Math.Abs(maxY - topY);
                double distReturnBot = 2.0 * Math.Abs(bottomY - minY);
                double distTraverse = L;

                if (i == 0)
                {
                    // Ä°LK KORÄ°DOR: Depodan doÄŸrudan alt koridora girilir
                    double dx = Math.Abs(currAisleX - depotMerkez.X);

                    // Sonunda ÃœSTTE (TOP) kalmak istiyorsak: Traverse Up (AÅŸaÄŸÄ±dan yukarÄ± tam geÃ§iÅŸ)
                    costTop[i] = dx + distTraverse;
                    pathTop[i] = new List<Point> {
                depotMerkez,
                new Point(depotMerkez.X, bottomY),
                new Point(currAisleX, bottomY),
                new Point(currAisleX, topY)
            };

                    // Sonunda ALTTA (BOTTOM) kalmak istiyorsak: Return Bot (AÅŸaÄŸÄ±dan gir, Ã¼rÃ¼nÃ¼ al, aÅŸaÄŸÄ± Ã§Ä±k)
                    costBot[i] = dx + distReturnBot;
                    pathBot[i] = new List<Point> {
                depotMerkez,
                new Point(depotMerkez.X, bottomY),
                new Point(currAisleX, bottomY),
                new Point(currAisleX, minY),
                new Point(currAisleX, bottomY)
            };
                }
                else
                {
                    // SONRAKÄ° KORÄ°DORLAR Ä°Ã‡Ä°N DÄ°NAMÄ°K KARAR AÄACI
                    int prevAisleX = aislesWithItems[i - 1];
                    double dx = Math.Abs(currAisleX - prevAisleX);

                    // --- A) KORÄ°DORU ÃœSTTE (TOP) BÄ°TÄ°RME MALÄ°YET HESAPLARI ---
                    double costTop_fromTop = costTop[i - 1] + dx + distReturnTop; // Ãœstten geldik, Ã¼rÃ¼nÃ¼ alÄ±p Ã¼ste dÃ¶ndÃ¼k
                    double costTop_fromBot = costBot[i - 1] + dx + distTraverse;  // Alttan geldik, koridoru boydan boya geÃ§ip Ã¼ste Ã§Ä±ktÄ±k

                    if (costTop_fromTop <= costTop_fromBot)
                    {
                        costTop[i] = costTop_fromTop;
                        pathTop[i] = new List<Point>(pathTop[i - 1]);
                        pathTop[i].Add(new Point(currAisleX, topY));
                        pathTop[i].Add(new Point(currAisleX, maxY)); // Tam Ã¼rÃ¼ne kadar in
                        pathTop[i].Add(new Point(currAisleX, topY)); // Geri dÃ¶n
                    }
                    else
                    {
                        costTop[i] = costTop_fromBot;
                        pathTop[i] = new List<Point>(pathBot[i - 1]);
                        pathTop[i].Add(new Point(currAisleX, bottomY));
                        pathTop[i].Add(new Point(currAisleX, topY)); // Boydan boya geÃ§
                    }

                    // --- B) KORÄ°DORU ALTTA (BOTTOM) BÄ°TÄ°RME MALÄ°YET HESAPLARI ---
                    double costBot_fromTop = costTop[i - 1] + dx + distTraverse;  // Ãœstten geldik, koridoru boydan boya geÃ§ip alta indik
                    double costBot_fromBot = costBot[i - 1] + dx + distReturnBot; // Alttan geldik, Ã¼rÃ¼nÃ¼ alÄ±p alta dÃ¶ndÃ¼k

                    if (costBot_fromTop <= costBot_fromBot)
                    {
                        costBot[i] = costBot_fromTop;
                        pathBot[i] = new List<Point>(pathTop[i - 1]);
                        pathBot[i].Add(new Point(currAisleX, topY));
                        pathBot[i].Add(new Point(currAisleX, bottomY)); // Boydan boya geÃ§
                    }
                    else
                    {
                        costBot[i] = costBot_fromBot;
                        pathBot[i] = new List<Point>(pathBot[i - 1]);
                        pathBot[i].Add(new Point(currAisleX, bottomY));
                        pathBot[i].Add(new Point(currAisleX, minY)); // Tam Ã¼rÃ¼ne kadar Ã§Ä±k
                        pathBot[i].Add(new Point(currAisleX, bottomY)); // Geri dÃ¶n
                    }
                }
            }

            // 5. SON KORÄ°DORDAN DEPOYA GÃœVENLÄ° DÃ–NÃœÅ
            // Matematiksel olarak Depo aÅŸaÄŸÄ±da olduÄŸu iÃ§in rotayÄ± altta (Bottom) bitirmek her zaman optimaldir.
            List<Point> finalRoute = new List<Point>(pathBot[N - 1]);

            // Alt hizada yÃ¼rÃ¼yerek depoya giriÅŸ yap
            finalRoute.Add(new Point(depotMerkez.X, bottomY));
            finalRoute.Add(depotMerkez);

            rotaYolu = finalRoute;

            // 6. Ã‡izgileri Boyama Ä°ÅŸlemi
            for (int i = 0; i < rotaYolu.Count - 1; i++)
            {
                ProcessPathSegment(rotaYolu[i], rotaYolu[i + 1]);
            }

            panelWarehouse.Invalidate();
            StoreRouteResult("Combined");
        }

        // ==========================================
        // MOUSE CLICK
        // ==========================================
        private void panelWarehouse_MouseClick(object sender, MouseEventArgs e)
        {
            if (!layoutHazir)
                return;

            if (cizimModu && Rectangle.Round(depotRect).Contains(e.Location))
            {
                FinishManualRoute();
                return;
            }

            if (!cizimModu)
            {
                var clickedCell = hucreler.FirstOrDefault(h => h.Bounds.Contains(e.Location));
                if (clickedCell != null)
                {
                    if (clickedCell.SelectedShelves.Contains(selectedShelf))
                    {
                        clickedCell.SelectedShelves.Remove(selectedShelf);
                        clickedCell.PickedShelves.Remove(selectedShelf);
                    }
                    else
                    {
                        clickedCell.SelectedShelves.Add(selectedShelf);
                    }

                    clickedCell.IsSelected = clickedCell.SelectedShelves.Count > 0;
                    clickedCell.IsPicked = clickedCell.PickedShelves.Count > 0;
                    rotaYolu.Clear();

                    foreach (var cp in crossoverNoktalari)
                        cp.IsVisited = false;

                    panelWarehouse.Invalidate();
                    return;
                }
            }

            // Depot'a tÄ±klayÄ±nca Ã§izim baÅŸlat
            if (Rectangle.Round(depotRect).Contains(e.Location))
            {
                StartManualRoute();
            }
        }

        // ==========================================
        // MOUSE MOVE
        // ==========================================
        private void panelWarehouse_MouseMove(object sender, MouseEventArgs e)
        {
            if (!layoutHazir)
                return;

            if (!cizimModu)
                return;

            Point mouse = e.Location;

            if (!warehouseBounds.Contains(mouse))
                return;

            Point? hedef = GetManualRouteTarget(mouse);

            if (!hedef.HasValue)
                return;

            AddManualRoutePoint(hedef.Value);
        }

        private Point? GetManualRouteTarget(Point mouse)
        {
            List<int> aisleXs = GetAisleXs();
            List<int> crossYs = GetCrossAisleYs();

            if (aisleXs.Count == 0 || crossYs.Count == 0 || rotaYolu.Count == 0)
                return null;

            if (rotaYolu.Count == 1)
            {
                int bottomY = crossYs.Last();
                Point depotToBottom = new Point(sonMouseNoktasi.X, bottomY);
                rotaYolu.Add(depotToBottom);
                sonMouseNoktasi = depotToBottom;
            }

            Point last = sonMouseNoktasi;
            bool onAisle = IsCloseToAny(aisleXs, last.X, 2);
            bool onCross = IsCloseToAny(crossYs, last.Y, 2);

            int dx = mouse.X - last.X;
            int dy = mouse.Y - last.Y;

            if (Math.Abs(dx) < 3 && Math.Abs(dy) < 3)
                return null;

            if (onAisle && (!onCross || Math.Abs(dy) >= Math.Abs(dx)))
            {
                int y = Math.Max(warehouseBounds.Top, Math.Min(warehouseBounds.Bottom, mouse.Y));
                return new Point(last.X, y);
            }

            if (onCross)
            {
                int x = Math.Max(warehouseBounds.Left, Math.Min(warehouseBounds.Right, mouse.X));
                int nearestAisleX = GetNearestValue(aisleXs, x);

                if (Math.Abs(nearestAisleX - x) <= 10)
                    x = nearestAisleX;

                return new Point(x, last.Y);
            }

            return null;
        }

        private void AddManualRoutePoint(Point target)
        {
            if (ManhattanDistance(sonMouseNoktasi, target) < 3)
                return;

            if (rotaYolu.Count >= 2)
            {
                Point previous = rotaYolu[rotaYolu.Count - 2];
                Point last = rotaYolu[rotaYolu.Count - 1];
                bool returningOnLastSegment =
                    IsPointOnSegment(target, previous, last) &&
                    ManhattanDistance(previous, target) < ManhattanDistance(previous, last);

                if (returningOnLastSegment)
                {
                    if (SegmentShouldBeKeptOnManualReturn(previous, last))
                    {
                        rotaYolu.Add(target);
                    }
                    else
                    {
                        if (target == previous)
                            rotaYolu.RemoveAt(rotaYolu.Count - 1);
                        else
                            rotaYolu[rotaYolu.Count - 1] = target;
                    }

                    sonMouseNoktasi = target;
                    RecalculateRouteEffects();
                    panelWarehouse.Invalidate();
                    return;
                }

                bool sameVertical = previous.X == last.X && last.X == target.X;
                bool sameHorizontal = previous.Y == last.Y && last.Y == target.Y;

                if (sameVertical || sameHorizontal)
                {
                    rotaYolu[rotaYolu.Count - 1] = target;
                    sonMouseNoktasi = target;
                    RecalculateRouteEffects();
                    panelWarehouse.Invalidate();
                    return;
                }
            }

            rotaYolu.Add(target);
            sonMouseNoktasi = target;
            RecalculateRouteEffects();
            panelWarehouse.Invalidate();
        }
        private void panelWarehouse_MouseClick_1(object sender, MouseEventArgs e)
        {

        }
    }
}


