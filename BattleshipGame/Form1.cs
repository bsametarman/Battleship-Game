namespace BattleshipGame
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }
        private Rectangle originalFromSize;
        private Rectangle originalOpGrupBoxSize;
        private void button8_Click(object sender, EventArgs e)
        {

        }

        private void Form1_Load(object sender, EventArgs e)
        {
            originalFromSize= new Rectangle(this.Location.X, this.Location.Y, this.Size.Width, this.Size.Height);
            originalOpGrupBoxSize= new Rectangle(opButtonGroupBox.Location.X, opButtonGroupBox.Location.Y, opButtonGroupBox.Width, opButtonGroupBox.Height);
        }

        private void sizeControl(Rectangle r, Control c)
        {
            float xRatio = (float)(this.Width) / (float)(originalFromSize.Width);
            float yRatio = (float)(this.Height) / (float)(originalFromSize.Height);

            int X = (int)(r.Location.X * xRatio);
            int Y = (int)(r.Location.Y * yRatio);

            int Width = (int)(r.Width * xRatio);
            int Height = (int)(r.Height * yRatio);

            c.Location = new Point(X, Y);
            c.Size = new Size(Width, Height);
        }

        private void Form1_Resize(object sender, EventArgs e)
        {
            sizeControl(originalOpGrupBoxSize, opButtonGroupBox);
        }
    }
}