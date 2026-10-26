namespace ibonundeposu.formapp
{
    partial class Form1
    {
        /// <summary>
        ///Gerekli tasarımcı değişkeni.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///Kullanılan tüm kaynakları temizleyin.
        /// </summary>
        ///<param name="disposing">yönetilen kaynaklar dispose edilmeliyse doğru; aksi halde yanlış.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer üretilen kod

        /// <summary>
        /// Tasarımcı desteği için gerekli metot - bu metodun 
        ///içeriğini kod düzenleyici ile değiştirmeyin.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form1));
            this.rbLayout = new System.Windows.Forms.Panel();
            this.radioButton5 = new System.Windows.Forms.RadioButton();
            this.radioButton4 = new System.Windows.Forms.RadioButton();
            this.radioButton3 = new System.Windows.Forms.RadioButton();
            this.radioButton2 = new System.Windows.Forms.RadioButton();
            this.radioButton1 = new System.Windows.Forms.RadioButton();
            this.pnlLayout = new System.Windows.Forms.Panel();
            this.txtCrossoverLength = new System.Windows.Forms.TextBox();
            this.txtAisleLength = new System.Windows.Forms.TextBox();
            this.txtDepot = new System.Windows.Forms.TextBox();
            this.txtLocations = new System.Windows.Forms.TextBox();
            this.txtAisles = new System.Windows.Forms.TextBox();
            this.txtBlocks = new System.Windows.Forms.TextBox();
            this.btnShowWarehouse = new System.Windows.Forms.Button();
            this.label8 = new System.Windows.Forms.Label();
            this.label7 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.pnlOrder = new System.Windows.Forms.Panel();
            this.button3 = new System.Windows.Forms.Button();
            this.label11 = new System.Windows.Forms.Label();
            this.button2 = new System.Windows.Forms.Button();
            this.textBox7 = new System.Windows.Forms.TextBox();
            this.label10 = new System.Windows.Forms.Label();
            this.label9 = new System.Windows.Forms.Label();
            this.pnlRoute = new System.Windows.Forms.Panel();
            this.button4 = new System.Windows.Forms.Button();
            this.label12 = new System.Windows.Forms.Label();
            this.label14 = new System.Windows.Forms.Label();
            this.pnlMethods = new System.Windows.Forms.Panel();
            this.button10 = new System.Windows.Forms.Button();
            this.button9 = new System.Windows.Forms.Button();
            this.button8 = new System.Windows.Forms.Button();
            this.button7 = new System.Windows.Forms.Button();
            this.button6 = new System.Windows.Forms.Button();
            this.button5 = new System.Windows.Forms.Button();
            this.label13 = new System.Windows.Forms.Label();
            this.label15 = new System.Windows.Forms.Label();
            this.pnlResults = new System.Windows.Forms.Panel();
            this.textBox17 = new System.Windows.Forms.TextBox();
            this.textBox16 = new System.Windows.Forms.TextBox();
            this.textBox15 = new System.Windows.Forms.TextBox();
            this.textBox14 = new System.Windows.Forms.TextBox();
            this.textBox13 = new System.Windows.Forms.TextBox();
            this.textBox12 = new System.Windows.Forms.TextBox();
            this.textBox11 = new System.Windows.Forms.TextBox();
            this.textBox10 = new System.Windows.Forms.TextBox();
            this.textBox9 = new System.Windows.Forms.TextBox();
            this.textBox8 = new System.Windows.Forms.TextBox();
            this.label16 = new System.Windows.Forms.Label();
            this.label17 = new System.Windows.Forms.Label();
            this.panel1 = new System.Windows.Forms.Panel();
            this.panelWarehouse = new System.Windows.Forms.Panel();
            this.textBox1 = new System.Windows.Forms.TextBox();
            this.label18 = new System.Windows.Forms.Label();
            this.label19 = new System.Windows.Forms.Label();
            this.textBox2 = new System.Windows.Forms.TextBox();
            this.rbLayout.SuspendLayout();
            this.pnlLayout.SuspendLayout();
            this.pnlOrder.SuspendLayout();
            this.pnlRoute.SuspendLayout();
            this.pnlMethods.SuspendLayout();
            this.pnlResults.SuspendLayout();
            this.panel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // rbLayout
            // 
            this.rbLayout.Controls.Add(this.radioButton5);
            this.rbLayout.Controls.Add(this.radioButton4);
            this.rbLayout.Controls.Add(this.radioButton3);
            this.rbLayout.Controls.Add(this.radioButton2);
            this.rbLayout.Controls.Add(this.radioButton1);
            this.rbLayout.Location = new System.Drawing.Point(3, 12);
            this.rbLayout.Name = "rbLayout";
            this.rbLayout.Size = new System.Drawing.Size(212, 121);
            this.rbLayout.TabIndex = 1;
            // 
            // radioButton5
            // 
            this.radioButton5.AutoSize = true;
            this.radioButton5.Location = new System.Drawing.Point(4, 96);
            this.radioButton5.Name = "radioButton5";
            this.radioButton5.Size = new System.Drawing.Size(60, 17);
            this.radioButton5.TabIndex = 4;
            this.radioButton5.Text = "Results";
            this.radioButton5.UseVisualStyleBackColor = true;
            this.radioButton5.CheckedChanged += new System.EventHandler(this.radioButton5_CheckedChanged);
            // 
            // radioButton4
            // 
            this.radioButton4.AutoSize = true;
            this.radioButton4.Location = new System.Drawing.Point(4, 73);
            this.radioButton4.Name = "radioButton4";
            this.radioButton4.Size = new System.Drawing.Size(106, 17);
            this.radioButton4.TabIndex = 3;
            this.radioButton4.Text = "Routing Methods";
            this.radioButton4.UseVisualStyleBackColor = true;
            this.radioButton4.CheckedChanged += new System.EventHandler(this.radioButton4_CheckedChanged);
            // 
            // radioButton3
            // 
            this.radioButton3.AutoSize = true;
            this.radioButton3.Location = new System.Drawing.Point(4, 50);
            this.radioButton3.Name = "radioButton3";
            this.radioButton3.Size = new System.Drawing.Size(92, 17);
            this.radioButton3.TabIndex = 2;
            this.radioButton3.Text = "Create a route";
            this.radioButton3.UseVisualStyleBackColor = true;
            this.radioButton3.CheckedChanged += new System.EventHandler(this.radioButton3_CheckedChanged);
            // 
            // radioButton2
            // 
            this.radioButton2.AutoSize = true;
            this.radioButton2.Location = new System.Drawing.Point(4, 27);
            this.radioButton2.Name = "radioButton2";
            this.radioButton2.Size = new System.Drawing.Size(98, 17);
            this.radioButton2.TabIndex = 1;
            this.radioButton2.Text = "Create an order";
            this.radioButton2.UseVisualStyleBackColor = true;
            this.radioButton2.CheckedChanged += new System.EventHandler(this.radioButton2_CheckedChanged);
            // 
            // radioButton1
            // 
            this.radioButton1.AutoSize = true;
            this.radioButton1.Checked = true;
            this.radioButton1.Location = new System.Drawing.Point(4, 4);
            this.radioButton1.Name = "radioButton1";
            this.radioButton1.Size = new System.Drawing.Size(76, 17);
            this.radioButton1.TabIndex = 0;
            this.radioButton1.TabStop = true;
            this.radioButton1.Text = "Set Layout";
            this.radioButton1.UseVisualStyleBackColor = true;
            this.radioButton1.CheckedChanged += new System.EventHandler(this.radioButton1_CheckedChanged);
            // 
            // pnlLayout
            // 
            this.pnlLayout.Controls.Add(this.textBox2);
            this.pnlLayout.Controls.Add(this.label19);
            this.pnlLayout.Controls.Add(this.label18);
            this.pnlLayout.Controls.Add(this.textBox1);
            this.pnlLayout.Controls.Add(this.txtCrossoverLength);
            this.pnlLayout.Controls.Add(this.txtAisleLength);
            this.pnlLayout.Controls.Add(this.txtDepot);
            this.pnlLayout.Controls.Add(this.txtLocations);
            this.pnlLayout.Controls.Add(this.txtAisles);
            this.pnlLayout.Controls.Add(this.txtBlocks);
            this.pnlLayout.Controls.Add(this.btnShowWarehouse);
            this.pnlLayout.Controls.Add(this.label8);
            this.pnlLayout.Controls.Add(this.label7);
            this.pnlLayout.Controls.Add(this.label6);
            this.pnlLayout.Controls.Add(this.label5);
            this.pnlLayout.Controls.Add(this.label4);
            this.pnlLayout.Controls.Add(this.label3);
            this.pnlLayout.Controls.Add(this.label2);
            this.pnlLayout.Controls.Add(this.label1);
            this.pnlLayout.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlLayout.Location = new System.Drawing.Point(0, 0);
            this.pnlLayout.Name = "pnlLayout";
            this.pnlLayout.Size = new System.Drawing.Size(212, 410);
            this.pnlLayout.TabIndex = 4;
            // 
            // txtCrossoverLength
            // 
            this.txtCrossoverLength.Location = new System.Drawing.Point(99, 204);
            this.txtCrossoverLength.Name = "txtCrossoverLength";
            this.txtCrossoverLength.Size = new System.Drawing.Size(100, 20);
            this.txtCrossoverLength.TabIndex = 12;
            // 
            // txtAisleLength
            // 
            this.txtAisleLength.Location = new System.Drawing.Point(99, 178);
            this.txtAisleLength.Name = "txtAisleLength";
            this.txtAisleLength.Size = new System.Drawing.Size(100, 20);
            this.txtAisleLength.TabIndex = 11;
            // 
            // txtDepot
            // 
            this.txtDepot.Location = new System.Drawing.Point(99, 152);
            this.txtDepot.Name = "txtDepot";
            this.txtDepot.Size = new System.Drawing.Size(100, 20);
            this.txtDepot.TabIndex = 10;
            // 
            // txtLocations
            // 
            this.txtLocations.Location = new System.Drawing.Point(99, 123);
            this.txtLocations.Name = "txtLocations";
            this.txtLocations.Size = new System.Drawing.Size(100, 20);
            this.txtLocations.TabIndex = 9;
            // 
            // txtAisles
            // 
            this.txtAisles.Location = new System.Drawing.Point(99, 97);
            this.txtAisles.Name = "txtAisles";
            this.txtAisles.Size = new System.Drawing.Size(100, 20);
            this.txtAisles.TabIndex = 8;
            // 
            // txtBlocks
            // 
            this.txtBlocks.Location = new System.Drawing.Point(99, 69);
            this.txtBlocks.Name = "txtBlocks";
            this.txtBlocks.Size = new System.Drawing.Size(100, 20);
            this.txtBlocks.TabIndex = 7;
            // 
            // btnShowWarehouse
            // 
            this.btnShowWarehouse.Location = new System.Drawing.Point(57, 297);
            this.btnShowWarehouse.Name = "btnShowWarehouse";
            this.btnShowWarehouse.Size = new System.Drawing.Size(75, 36);
            this.btnShowWarehouse.TabIndex = 6;
            this.btnShowWarehouse.Text = "show warehouse";
            this.btnShowWarehouse.UseVisualStyleBackColor = true;
            this.btnShowWarehouse.Click += new System.EventHandler(this.btnShowWarehouse_Click);
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F);
            this.label8.Location = new System.Drawing.Point(4, 202);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(85, 26);
            this.label8.TabIndex = 5;
            this.label8.Text = "crossover length\r\n(optional)";
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F);
            this.label7.Location = new System.Drawing.Point(4, 178);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(63, 26);
            this.label7.TabIndex = 5;
            this.label7.Text = "aisle length \r\n(optional)";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F);
            this.label6.Location = new System.Drawing.Point(4, 155);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(74, 13);
            this.label6.TabIndex = 5;
            this.label6.Text = "depot location";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F);
            this.label5.Location = new System.Drawing.Point(4, 117);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(91, 26);
            this.label5.TabIndex = 4;
            this.label5.Text = "number of \r\nlocations per aisle";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F);
            this.label4.Location = new System.Drawing.Point(4, 93);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(83, 13);
            this.label4.TabIndex = 3;
            this.label4.Text = "number of aisles";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F);
            this.label3.Location = new System.Drawing.Point(4, 69);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(88, 13);
            this.label3.TabIndex = 2;
            this.label3.Text = "number of blocks";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F);
            this.label2.Location = new System.Drawing.Point(7, 29);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(134, 26);
            this.label2.TabIndex = 1;
            this.label2.Text = "Fiil out this form , then click\r\non \"show warehouse\".";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(162)));
            this.label1.Location = new System.Drawing.Point(4, 12);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(84, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "SET LAYOUT";
            // 
            // pnlOrder
            // 
            this.pnlOrder.Controls.Add(this.button3);
            this.pnlOrder.Controls.Add(this.label11);
            this.pnlOrder.Controls.Add(this.button2);
            this.pnlOrder.Controls.Add(this.textBox7);
            this.pnlOrder.Controls.Add(this.label10);
            this.pnlOrder.Controls.Add(this.label9);
            this.pnlOrder.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlOrder.Location = new System.Drawing.Point(0, 0);
            this.pnlOrder.Name = "pnlOrder";
            this.pnlOrder.Size = new System.Drawing.Size(212, 410);
            this.pnlOrder.TabIndex = 13;
            // 
            // button3
            // 
            this.button3.Location = new System.Drawing.Point(57, 241);
            this.button3.Name = "button3";
            this.button3.Size = new System.Drawing.Size(75, 23);
            this.button3.TabIndex = 5;
            this.button3.Text = "clear order";
            this.button3.UseVisualStyleBackColor = true;
            this.button3.Click += new System.EventHandler(this.button3_Click);
            // 
            // label11
            // 
            this.label11.AutoSize = true;
            this.label11.Location = new System.Drawing.Point(7, 163);
            this.label11.Name = "label11";
            this.label11.Size = new System.Drawing.Size(158, 65);
            this.label11.TabIndex = 4;
            this.label11.Text = "You can also create an order \r\nyourself , by clicking on \r\nthe locations of the w" +
    "arehouse .\r\nTo clear all locations , \r\nclick this button :\r\n";
            // 
            // button2
            // 
            this.button2.Location = new System.Drawing.Point(99, 121);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(75, 23);
            this.button2.TabIndex = 3;
            this.button2.Text = "set order";
            this.button2.UseVisualStyleBackColor = true;
            this.button2.Click += new System.EventHandler(this.button2_Click);
            // 
            // textBox7
            // 
            this.textBox7.Location = new System.Drawing.Point(24, 123);
            this.textBox7.Name = "textBox7";
            this.textBox7.Size = new System.Drawing.Size(54, 20);
            this.textBox7.TabIndex = 2;
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Location = new System.Drawing.Point(4, 41);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(150, 65);
            this.label10.TabIndex = 1;
            this.label10.Text = "First type the number of \r\nproducts you would you like to\r\n pick . Then click on " +
    "the \r\n\"set order\" button to create a\r\n random order.";
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(162)));
            this.label9.Location = new System.Drawing.Point(4, 10);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(95, 13);
            this.label9.TabIndex = 0;
            this.label9.Text = "Create an order";
            // 
            // pnlRoute
            // 
            this.pnlRoute.Controls.Add(this.button4);
            this.pnlRoute.Controls.Add(this.label12);
            this.pnlRoute.Controls.Add(this.label14);
            this.pnlRoute.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlRoute.Location = new System.Drawing.Point(0, 0);
            this.pnlRoute.Name = "pnlRoute";
            this.pnlRoute.Size = new System.Drawing.Size(212, 410);
            this.pnlRoute.TabIndex = 14;
            // 
            // button4
            // 
            this.button4.Location = new System.Drawing.Point(57, 197);
            this.button4.Name = "button4";
            this.button4.Size = new System.Drawing.Size(75, 23);
            this.button4.TabIndex = 2;
            this.button4.Text = "new route";
            this.button4.UseVisualStyleBackColor = true;
            // 
            // label12
            // 
            this.label12.AutoSize = true;
            this.label12.Location = new System.Drawing.Point(7, 41);
            this.label12.Name = "label12";
            this.label12.Size = new System.Drawing.Size(155, 130);
            this.label12.TabIndex = 1;
            this.label12.Text = resources.GetString("label12.Text");
            // 
            // label14
            // 
            this.label14.AutoSize = true;
            this.label14.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(162)));
            this.label14.Location = new System.Drawing.Point(4, 10);
            this.label14.Name = "label14";
            this.label14.Size = new System.Drawing.Size(88, 13);
            this.label14.TabIndex = 0;
            this.label14.Text = "Create a route";
            // 
            // pnlMethods
            // 
            this.pnlMethods.Controls.Add(this.button10);
            this.pnlMethods.Controls.Add(this.button9);
            this.pnlMethods.Controls.Add(this.button8);
            this.pnlMethods.Controls.Add(this.button7);
            this.pnlMethods.Controls.Add(this.button6);
            this.pnlMethods.Controls.Add(this.button5);
            this.pnlMethods.Controls.Add(this.label13);
            this.pnlMethods.Controls.Add(this.label15);
            this.pnlMethods.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlMethods.Location = new System.Drawing.Point(0, 0);
            this.pnlMethods.Name = "pnlMethods";
            this.pnlMethods.Size = new System.Drawing.Size(212, 410);
            this.pnlMethods.TabIndex = 15;
            // 
            // button10
            // 
            this.button10.Location = new System.Drawing.Point(57, 260);
            this.button10.Name = "button10";
            this.button10.Size = new System.Drawing.Size(75, 23);
            this.button10.TabIndex = 7;
            this.button10.Text = "Combined +";
            this.button10.UseVisualStyleBackColor = true;
            // 
            // button9
            // 
            this.button9.Location = new System.Drawing.Point(57, 231);
            this.button9.Name = "button9";
            this.button9.Size = new System.Drawing.Size(75, 23);
            this.button9.TabIndex = 6;
            this.button9.Text = "Aisle-by-aisle";
            this.button9.UseVisualStyleBackColor = true;
            // 
            // button8
            // 
            this.button8.Location = new System.Drawing.Point(57, 202);
            this.button8.Name = "button8";
            this.button8.Size = new System.Drawing.Size(75, 23);
            this.button8.TabIndex = 5;
            this.button8.Text = "Largest Gap";
            this.button8.UseVisualStyleBackColor = true;
            // 
            // button7
            // 
            this.button7.Location = new System.Drawing.Point(57, 174);
            this.button7.Name = "button7";
            this.button7.Size = new System.Drawing.Size(75, 23);
            this.button7.TabIndex = 4;
            this.button7.Text = "Combined";
            this.button7.UseVisualStyleBackColor = true;
            this.button7.Click += new System.EventHandler(this.button7_Click);
            // 
            // button6
            // 
            this.button6.Location = new System.Drawing.Point(57, 145);
            this.button6.Name = "button6";
            this.button6.Size = new System.Drawing.Size(75, 23);
            this.button6.TabIndex = 3;
            this.button6.Text = "S-shape";
            this.button6.UseVisualStyleBackColor = true;
            this.button6.Click += new System.EventHandler(this.button6_Click);
            // 
            // button5
            // 
            this.button5.Location = new System.Drawing.Point(57, 116);
            this.button5.Name = "button5";
            this.button5.Size = new System.Drawing.Size(75, 23);
            this.button5.TabIndex = 2;
            this.button5.Text = "Optimal";
            this.button5.UseVisualStyleBackColor = true;
            // 
            // label13
            // 
            this.label13.AutoSize = true;
            this.label13.Location = new System.Drawing.Point(7, 41);
            this.label13.Name = "label13";
            this.label13.Size = new System.Drawing.Size(139, 52);
            this.label13.TabIndex = 1;
            this.label13.Text = "Click on any of the following\r\nbuttons to this play an order\r\npicking route corre" +
    "sponding\r\nto that routing method.";
            // 
            // label15
            // 
            this.label15.AutoSize = true;
            this.label15.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(162)));
            this.label15.Location = new System.Drawing.Point(4, 10);
            this.label15.Name = "label15";
            this.label15.Size = new System.Drawing.Size(102, 13);
            this.label15.TabIndex = 0;
            this.label15.Text = "Routing methods";
            // 
            // pnlResults
            // 
            this.pnlResults.Controls.Add(this.textBox17);
            this.pnlResults.Controls.Add(this.textBox16);
            this.pnlResults.Controls.Add(this.textBox15);
            this.pnlResults.Controls.Add(this.textBox14);
            this.pnlResults.Controls.Add(this.textBox13);
            this.pnlResults.Controls.Add(this.textBox12);
            this.pnlResults.Controls.Add(this.textBox11);
            this.pnlResults.Controls.Add(this.textBox10);
            this.pnlResults.Controls.Add(this.textBox9);
            this.pnlResults.Controls.Add(this.textBox8);
            this.pnlResults.Controls.Add(this.label16);
            this.pnlResults.Controls.Add(this.label17);
            this.pnlResults.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlResults.Location = new System.Drawing.Point(0, 0);
            this.pnlResults.Name = "pnlResults";
            this.pnlResults.Size = new System.Drawing.Size(212, 410);
            this.pnlResults.TabIndex = 16;
            // 
            // textBox17
            // 
            this.textBox17.Location = new System.Drawing.Point(99, 363);
            this.textBox17.Name = "textBox17";
            this.textBox17.Size = new System.Drawing.Size(100, 20);
            this.textBox17.TabIndex = 15;
            // 
            // textBox16
            // 
            this.textBox16.Location = new System.Drawing.Point(99, 337);
            this.textBox16.Name = "textBox16";
            this.textBox16.Size = new System.Drawing.Size(100, 20);
            this.textBox16.TabIndex = 14;
            // 
            // textBox15
            // 
            this.textBox15.Location = new System.Drawing.Point(99, 297);
            this.textBox15.Name = "textBox15";
            this.textBox15.Size = new System.Drawing.Size(100, 20);
            this.textBox15.TabIndex = 13;
            // 
            // textBox14
            // 
            this.textBox14.Location = new System.Drawing.Point(99, 255);
            this.textBox14.Name = "textBox14";
            this.textBox14.Size = new System.Drawing.Size(100, 20);
            this.textBox14.TabIndex = 12;
            // 
            // textBox13
            // 
            this.textBox13.Location = new System.Drawing.Point(99, 215);
            this.textBox13.Name = "textBox13";
            this.textBox13.Size = new System.Drawing.Size(100, 20);
            this.textBox13.TabIndex = 11;
            // 
            // textBox12
            // 
            this.textBox12.Location = new System.Drawing.Point(99, 177);
            this.textBox12.Name = "textBox12";
            this.textBox12.Size = new System.Drawing.Size(100, 20);
            this.textBox12.TabIndex = 10;
            // 
            // textBox11
            // 
            this.textBox11.Location = new System.Drawing.Point(99, 140);
            this.textBox11.Name = "textBox11";
            this.textBox11.Size = new System.Drawing.Size(100, 20);
            this.textBox11.TabIndex = 9;
            // 
            // textBox10
            // 
            this.textBox10.Location = new System.Drawing.Point(99, 97);
            this.textBox10.Name = "textBox10";
            this.textBox10.Size = new System.Drawing.Size(100, 20);
            this.textBox10.TabIndex = 8;
            // 
            // textBox9
            // 
            this.textBox9.Location = new System.Drawing.Point(99, 62);
            this.textBox9.Name = "textBox9";
            this.textBox9.Size = new System.Drawing.Size(100, 20);
            this.textBox9.TabIndex = 7;
            // 
            // textBox8
            // 
            this.textBox8.Location = new System.Drawing.Point(99, 26);
            this.textBox8.Name = "textBox8";
            this.textBox8.Size = new System.Drawing.Size(100, 20);
            this.textBox8.TabIndex = 6;
            // 
            // label16
            // 
            this.label16.AutoSize = true;
            this.label16.Location = new System.Drawing.Point(4, 25);
            this.label16.Name = "label16";
            this.label16.Size = new System.Drawing.Size(77, 351);
            this.label16.TabIndex = 5;
            this.label16.Text = "route1\r\n\r\n\r\nroute2\r\n\r\n\r\nroute3\r\n\r\n\r\nyour best route\r\n\r\n\r\nOptimal\r\n\r\n\r\nS-shape\r\n\r\n" +
    "\r\nCombined\r\n\r\n\r\nLargest Gap\r\n\r\n\r\nAisle-by-aisle\r\n\r\nCombined +";
            // 
            // label17
            // 
            this.label17.AutoSize = true;
            this.label17.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(162)));
            this.label17.Location = new System.Drawing.Point(4, 10);
            this.label17.Name = "label17";
            this.label17.Size = new System.Drawing.Size(49, 13);
            this.label17.TabIndex = 0;
            this.label17.Text = "Results";
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.pnlLayout);
            this.panel1.Controls.Add(this.pnlResults);
            this.panel1.Controls.Add(this.pnlRoute);
            this.panel1.Controls.Add(this.pnlMethods);
            this.panel1.Controls.Add(this.pnlOrder);
            this.panel1.Location = new System.Drawing.Point(3, 132);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(212, 410);
            this.panel1.TabIndex = 17;
            // 
            // panelWarehouse
            // 
            this.panelWarehouse.Location = new System.Drawing.Point(222, 12);
            this.panelWarehouse.Name = "panelWarehouse";
            this.panelWarehouse.Size = new System.Drawing.Size(716, 530);
            this.panelWarehouse.TabIndex = 18;
            this.panelWarehouse.MouseClick += new System.Windows.Forms.MouseEventHandler(this.panelWarehouse_MouseClick_1);
            this.panelWarehouse.MouseMove += new System.Windows.Forms.MouseEventHandler(this.panelWarehouse_MouseMove);
            // 
            // textBox1
            // 
            this.textBox1.Location = new System.Drawing.Point(99, 234);
            this.textBox1.Name = "textBox1";
            this.textBox1.Size = new System.Drawing.Size(100, 20);
            this.textBox1.TabIndex = 13;
            // 
            // label18
            // 
            this.label18.AutoSize = true;
            this.label18.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F);
            this.label18.Location = new System.Drawing.Point(11, 234);
            this.label18.Name = "label18";
            this.label18.Size = new System.Drawing.Size(47, 13);
            this.label18.TabIndex = 14;
            this.label18.Text = "raf sayısı";
            // 
            // label19
            // 
            this.label19.AutoSize = true;
            this.label19.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F);
            this.label19.Location = new System.Drawing.Point(7, 265);
            this.label19.Name = "label19";
            this.label19.Size = new System.Drawing.Size(51, 13);
            this.label19.TabIndex = 15;
            this.label19.Text = "yükseklik";
            // 
            // textBox2
            // 
            this.textBox2.Location = new System.Drawing.Point(99, 263);
            this.textBox2.Name = "textBox2";
            this.textBox2.Size = new System.Drawing.Size(100, 20);
            this.textBox2.TabIndex = 16;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(950, 554);
            this.Controls.Add(this.panelWarehouse);
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.rbLayout);
            this.Name = "Form1";
            this.Text = "Form1";
            this.rbLayout.ResumeLayout(false);
            this.rbLayout.PerformLayout();
            this.pnlLayout.ResumeLayout(false);
            this.pnlLayout.PerformLayout();
            this.pnlOrder.ResumeLayout(false);
            this.pnlOrder.PerformLayout();
            this.pnlRoute.ResumeLayout(false);
            this.pnlRoute.PerformLayout();
            this.pnlMethods.ResumeLayout(false);
            this.pnlMethods.PerformLayout();
            this.pnlResults.ResumeLayout(false);
            this.pnlResults.PerformLayout();
            this.panel1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel rbLayout;
        private System.Windows.Forms.RadioButton radioButton4;
        private System.Windows.Forms.RadioButton radioButton3;
        private System.Windows.Forms.RadioButton radioButton2;
        private System.Windows.Forms.RadioButton radioButton1;
        private System.Windows.Forms.RadioButton radioButton5;
        private System.Windows.Forms.Panel pnlLayout;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox txtCrossoverLength;
        private System.Windows.Forms.TextBox txtAisleLength;
        private System.Windows.Forms.TextBox txtDepot;
        private System.Windows.Forms.TextBox txtLocations;
        private System.Windows.Forms.TextBox txtAisles;
        private System.Windows.Forms.TextBox txtBlocks;
        private System.Windows.Forms.Button btnShowWarehouse;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.Panel pnlOrder;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.Panel pnlRoute;
        private System.Windows.Forms.Label label14;
        private System.Windows.Forms.Button button3;
        private System.Windows.Forms.Label label11;
        private System.Windows.Forms.Button button2;
        private System.Windows.Forms.TextBox textBox7;
        private System.Windows.Forms.Label label12;
        private System.Windows.Forms.Panel pnlMethods;
        private System.Windows.Forms.Button button5;
        private System.Windows.Forms.Label label13;
        private System.Windows.Forms.Label label15;
        private System.Windows.Forms.Button button4;
        private System.Windows.Forms.Button button10;
        private System.Windows.Forms.Button button9;
        private System.Windows.Forms.Button button8;
        private System.Windows.Forms.Button button7;
        private System.Windows.Forms.Button button6;
        private System.Windows.Forms.Panel pnlResults;
        private System.Windows.Forms.TextBox textBox17;
        private System.Windows.Forms.TextBox textBox16;
        private System.Windows.Forms.TextBox textBox15;
        private System.Windows.Forms.TextBox textBox14;
        private System.Windows.Forms.TextBox textBox13;
        private System.Windows.Forms.TextBox textBox12;
        private System.Windows.Forms.TextBox textBox11;
        private System.Windows.Forms.TextBox textBox10;
        private System.Windows.Forms.TextBox textBox9;
        private System.Windows.Forms.TextBox textBox8;
        private System.Windows.Forms.Label label16;
        private System.Windows.Forms.Label label17;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Panel panelWarehouse;
        private System.Windows.Forms.TextBox textBox2;
        private System.Windows.Forms.Label label19;
        private System.Windows.Forms.Label label18;
        private System.Windows.Forms.TextBox textBox1;
    }
}

