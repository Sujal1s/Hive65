using System;
using System.IO;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.Win32.SafeHandles;

public class KeyDef {
 public string Name; public int X,Y,W,H,Id;
 public KeyDef(string n,int x,int y,int w,int h,int id){Name=n;X=x;Y=y;W=w;H=h;Id=id;}
 public int Zone { get {return Math.Min(5,Math.Max(0,(X+W/2-19)*6/613));} }
}
public static class KeyboardLayout {
 public static KeyDef[] Keys = new KeyDef[]{
new KeyDef("Esc",19,29,34,34,1),
new KeyDef("1",56,29,34,34,7),
new KeyDef("2",95,29,34,34,13),
new KeyDef("3",134,30,34,34,19),
new KeyDef("4",173,30,34,34,25),
new KeyDef("5",212,30,34,34,31),
new KeyDef("6",250,30,34,34,37),
new KeyDef("7",289,30,34,34,43),
new KeyDef("8",328,30,34,34,49),
new KeyDef("9",367,30,34,34,55),
new KeyDef("0",406,30,34,34,61),
new KeyDef("-",445,30,34,34,67),
new KeyDef("=",484,30,34,34,73),
new KeyDef("Back",521,30,73,34,79),
new KeyDef("Del",598,69,34,34,92),
new KeyDef("Tab",20,69,52,34,2),
new KeyDef("Q",77,69,34,34,8),
new KeyDef("W",115,69,34,34,14),
new KeyDef("E",154,69,34,34,20),
new KeyDef("R",192,69,34,34,26),
new KeyDef("T",231,69,34,34,32),
new KeyDef("Y",270,69,34,34,38),
new KeyDef("U",309,69,34,34,44),
new KeyDef("I",348,69,34,34,50),
new KeyDef("O",386,69,34,34,56),
new KeyDef("P",425,69,34,34,62),
new KeyDef("[",463,69,34,34,68),
new KeyDef("]",502,69,34,34,74),
new KeyDef("\\",540,69,54,34,80),
new KeyDef("PgU",598,106,34,34,93),
new KeyDef("Caps",20,107,63,34,3),
new KeyDef("A",87,107,34,34,9),
new KeyDef("S",126,107,34,34,15),
new KeyDef("D",165,107,34,34,21),
new KeyDef("F",203,107,34,34,27),
new KeyDef("G",242,107,34,34,33),
new KeyDef("H",280,107,34,34,39),
new KeyDef("J",318,107,34,34,45),
new KeyDef("K",358,107,34,34,51),
new KeyDef("L",396,107,34,34,57),
new KeyDef(";",435,107,34,34,63),
new KeyDef("'",473,107,34,34,69),
new KeyDef("Enter",513,107,80,34,81),
new KeyDef("PgD",598,145,34,34,94),
new KeyDef("LShift",20,146,80,34,4),
new KeyDef("Z",106,146,34,34,10),
new KeyDef("X",145,146,34,34,16),
new KeyDef("C",183,146,34,34,22),
new KeyDef("V",221,146,34,34,28),
new KeyDef("B",260,146,34,34,34),
new KeyDef("N",299,146,34,34,40),
new KeyDef("M",338,146,34,34,46),
new KeyDef(",",375,146,34,34,52),
new KeyDef(".",415,146,34,34,58),
new KeyDef("/",454,146,34,34,64),
new KeyDef("RShift",491,146,65,34,82),
new KeyDef("Up",559,145,34,34,88),
new KeyDef("LCtrl",20,185,41,34,5),
new KeyDef("LWin",69,185,41,34,11),
new KeyDef("LAlt",117,185,41,34,17),
new KeyDef("Space",166,186,232,34,35),
new KeyDef("RAlt",406,185,34,34,53),
new KeyDef("FN",444,185,34,34,59),
new KeyDef("Left",523,184,34,34,83),
new KeyDef("v",560,184,34,34,89),
new KeyDef(">",598,184,34,34,95),
new KeyDef("Ctrl",482,185,34,34,65),

 };
}
public static class Protocol {
 // Fw=24 -> vtable 0x61764c. Connected flag +0x234 selects raw
 // 378-byte RGB matrix at VA 0x483eee-0x483f07, NOT grouped RGB.
 // SetRealData 0x491100 -> 0x48ff00, report 6, command 8.
 public static byte[] Frame(Color[] colors) {
  if(colors==null || colors.Length!=KeyboardLayout.Keys.Length) throw new ArgumentException("One color per visible key required");
  var b=new byte[520];b[0]=6;b[1]=8;b[4]=1;b[6]=0x7a;b[7]=1;
  for(int n=0;n<KeyboardLayout.Keys.Length;n++) {var k=KeyboardLayout.Keys[n];int i=8+k.Id*3;Color c=colors[n];b[i]=c.R;b[i+1]=c.G;b[i+2]=c.B;}
  return b;
 }

}
public sealed class HidDevice:IDisposable {
 SafeFileHandle handle;
 [StructLayout(LayoutKind.Sequential)] struct Interface {public int Size;public Guid ClassGuid;public int Flags;public IntPtr Reserved;}
 [StructLayout(LayoutKind.Sequential)] struct Attr {public int Size;public ushort Vendor,Product,Version;}
 [DllImport("hid.dll")] static extern void HidD_GetHidGuid(out Guid g);
 [DllImport("hid.dll")] static extern bool HidD_GetAttributes(SafeFileHandle h,ref Attr a);
 [DllImport("hid.dll")] static extern bool HidD_GetPreparsedData(SafeFileHandle h,out IntPtr p);
 [DllImport("hid.dll")] static extern bool HidD_FreePreparsedData(IntPtr p);
 [DllImport("hid.dll")] static extern int HidP_GetCaps(IntPtr p,IntPtr caps);
 [DllImport("hid.dll",SetLastError=true)] static extern bool HidD_SetFeature(SafeFileHandle h,byte[] b,int n);
 [DllImport("setupapi.dll",CharSet=CharSet.Unicode,SetLastError=true)] static extern IntPtr SetupDiGetClassDevs(ref Guid g,string e,IntPtr w,uint f);
 [DllImport("setupapi.dll",SetLastError=true)] static extern bool SetupDiEnumDeviceInterfaces(IntPtr s,IntPtr d,ref Guid g,uint i,ref Interface v);
 [DllImport("setupapi.dll",CharSet=CharSet.Unicode,SetLastError=true)] static extern bool SetupDiGetDeviceInterfaceDetail(IntPtr s,ref Interface v,IntPtr detail,uint size,out uint required,IntPtr d);
 [DllImport("setupapi.dll")] static extern bool SetupDiDestroyDeviceInfoList(IntPtr s);
 [DllImport("kernel32.dll",CharSet=CharSet.Unicode,SetLastError=true)] static extern SafeFileHandle CreateFile(string p,uint a,uint share,IntPtr sec,uint creation,uint flags,IntPtr t);
 public static HidDevice Connect() {
  Guid g;HidD_GetHidGuid(out g);IntPtr s=SetupDiGetClassDevs(ref g,null,IntPtr.Zero,18);
  if(s==new IntPtr(-1))throw new IOException("Cannot enumerate USB devices");
  try {for(uint i=0;;i++) {
   var v=new Interface();v.Size=Marshal.SizeOf(typeof(Interface));
   if(!SetupDiEnumDeviceInterfaces(s,IntPtr.Zero,ref g,i,ref v))break;
   uint n;SetupDiGetDeviceInterfaceDetail(s,ref v,IntPtr.Zero,0,out n,IntPtr.Zero);
   if(n<8 || n>65536)continue;IntPtr d=Marshal.AllocHGlobal((int)n);
   try {Marshal.WriteInt32(d,IntPtr.Size==8?8:6);
    if(!SetupDiGetDeviceInterfaceDetail(s,ref v,d,n,out n,IntPtr.Zero))continue;
    string path=Marshal.PtrToStringUni(IntPtr.Add(d,4));
    if(path==null || path.IndexOf("vid_258a&pid_010c",StringComparison.OrdinalIgnoreCase)<0)continue;
    var h=CreateFile(path,0xC0000000,3,IntPtr.Zero,3,0,IntPtr.Zero);
    if(h.IsInvalid){h.Dispose();continue;}
    bool keep=false;
    try {var a=new Attr();a.Size=Marshal.SizeOf(typeof(Attr));
     if(!HidD_GetAttributes(h,ref a)||a.Vendor!=0x258a||a.Product!=0x010c)continue;
     IntPtr p;if(!HidD_GetPreparsedData(h,out p))continue;
     try {IntPtr caps=Marshal.AllocHGlobal(64);try {
      if(HidP_GetCaps(p,caps)!=0x110000)continue;
      // Usage=1, page=FF00, feature length=520: exact vendor driver selector.
      if((ushort)Marshal.ReadInt16(caps,0)!=1||(ushort)Marshal.ReadInt16(caps,2)!=0xff00||(ushort)Marshal.ReadInt16(caps,8)!=520)continue;
      keep=true;return new HidDevice{handle=h};
     }finally{Marshal.FreeHGlobal(caps);}}finally{HidD_FreePreparsedData(p);}
    }finally{if(!keep)h.Dispose();}
   }finally{Marshal.FreeHGlobal(d);}
  }}finally{SetupDiDestroyDeviceInfoList(s);}
  throw new IOException("Hive 65 USB interface not found. Connect by cable and close the Kreo app.");
 }
 public void Send(Color[] colors){byte[] b=Protocol.Frame(colors);if(!HidD_SetFeature(handle,b,b.Length))throw new IOException("USB write failed ("+Marshal.GetLastWin32Error()+"). Reconnect the keyboard.");}
 public void Dispose(){if(handle!=null){handle.Dispose();handle=null;}}
}
public static class ScreenSampler {
 public static Color[] Sample(Rectangle rect,int mapping=0,bool vivid=true) {
  using(var full=new Bitmap(rect.Width,rect.Height)) {
   using(var g=Graphics.FromImage(full))g.CopyFromScreen(rect.Location,Point.Empty,rect.Size);
   using(var small=new Bitmap(192,108)) {
    using(var g=Graphics.FromImage(small)){g.InterpolationMode=InterpolationMode.Bilinear;g.DrawImage(full,new Rectangle(0,0,192,108));}
    return Map(small,mapping,vivid);
   }
  }
 }
 public static Color[] Average(Bitmap b) {
  var result=new Color[6];
  for(int z=0;z<6;z++){long r=0,g=0,bl=0,n=0;for(int x=z*b.Width/6;x<(z+1)*b.Width/6;x++)for(int y=0;y<b.Height;y++){var c=b.GetPixel(x,y);r+=c.R;g+=c.G;bl+=c.B;n++;}result[z]=Color.FromArgb((int)(r/n),(int)(g/n),(int)(bl/n));}return result;
 }
 public static Rectangle Region(KeyDef k,int width,int height) {
  int x=Math.Max(0,(k.X-19)*width/613),y=Math.Max(0,(k.Y-29)*height/191);
  int right=Math.Min(width,(int)Math.Ceiling((k.X+k.W-19)*width/613.0)),bottom=Math.Min(height,(int)Math.Ceiling((k.Y+k.H-29)*height/191.0));
  return new Rectangle(x,y,Math.Max(1,right-x),Math.Max(1,bottom-y));
 }
 static readonly double[] Linear=MakeLinear();static double[] MakeLinear(){var a=new double[256];for(int i=0;i<256;i++){double v=i/255.0;a[i]=v<=0.04045?v/12.92:Math.Pow((v+0.055)/1.055,2.4);}return a;}
 static int Encode(double v){return Math.Max(0,Math.Min(255,(int)Math.Round(255*(v<=0.0031308?12.92*v:1.055*Math.Pow(v,1/2.4)-0.055))));}
 public static Color Area(Bitmap b,Rectangle r,bool vivid){double rr=0,gg=0,bb=0,total=0;
  for(int y=r.Top;y<r.Bottom;y++)for(int x=r.Left;x<r.Right;x++){Color c=b.GetPixel(x,y);int max=Math.Max(c.R,Math.Max(c.G,c.B)),min=Math.Min(c.R,Math.Min(c.G,c.B));double weight=1+(vivid&&max>0?0.8*(max-min)/(double)max:0);rr+=Linear[c.R]*weight;gg+=Linear[c.G]*weight;bb+=Linear[c.B]*weight;total+=weight;}
  return Color.FromArgb(Encode(rr/total),Encode(gg/total),Encode(bb/total));
 }
 public static Color[] Map(Bitmap b,int mapping,bool vivid){var a=new Color[KeyboardLayout.Keys.Length];Color[] zones=null;Color whole=Color.Black;
  if(mapping==1){zones=new Color[6];for(int z=0;z<6;z++)zones[z]=Area(b,new Rectangle(z*b.Width/6,0,(z+1)*b.Width/6-z*b.Width/6,b.Height),vivid);}
  if(mapping==2)whole=Area(b,new Rectangle(0,0,b.Width,b.Height),vivid);
  for(int n=0;n<a.Length;n++){var k=KeyboardLayout.Keys[n];a[n]=mapping==2?whole:mapping==1?zones[k.Zone]:Area(b,Region(k,b.Width,b.Height),vivid);}return a;
 }
 public static Color Blend(Color a,Color b,double amount,double brightness){return Color.FromArgb((int)Math.Round(a.R*(1-amount)+b.R*brightness*amount),(int)Math.Round(a.G*(1-amount)+b.G*brightness*amount),(int)Math.Round(a.B*(1-amount)+b.B*brightness*amount));}
}
public static class Theme {
 public static Color Paper=Color.FromArgb(16,15,20), Ink=Color.FromArgb(239,235,247), Muted=Color.FromArgb(155,148,170), Line=Color.FromArgb(48,43,59), Accent=Color.FromArgb(117,64,221);
 public static Font Type(float size,FontStyle style=FontStyle.Regular){return new Font("Segoe UI",size,style);}
 public static void Text(Graphics g,string text,float size,Color color,Rectangle r,FontStyle style=FontStyle.Regular,TextFormatFlags flags=TextFormatFlags.Left|TextFormatFlags.VerticalCenter){using(var f=Type(size))TextRenderer.DrawText(g,text,f,r,color,flags|TextFormatFlags.NoPadding);}
 public static GraphicsPath Round(RectangleF r,float radius){var p=new GraphicsPath();float d=radius*2;p.AddArc(r.X,r.Y,d,d,180,90);p.AddArc(r.Right-d,r.Y,d,d,270,90);p.AddArc(r.Right-d,r.Bottom-d,d,d,0,90);p.AddArc(r.X,r.Bottom-d,d,d,90,90);p.CloseFigure();return p;}
}
public class QuietButton:Button {
 public bool Primary;bool hover;
 public QuietButton(){SetStyle(ControlStyles.UserPaint|ControlStyles.AllPaintingInWmPaint|ControlStyles.OptimizedDoubleBuffer,true);Cursor=Cursors.Hand;}
 protected override void OnMouseEnter(EventArgs e){hover=true;Invalidate();base.OnMouseEnter(e);}protected override void OnMouseLeave(EventArgs e){hover=false;Invalidate();base.OnMouseLeave(e);}
 protected override void OnPaint(PaintEventArgs e){Color fill=Primary?(hover?Color.FromArgb(136,84,239):Theme.Accent):(hover?Color.FromArgb(42,35,56):Color.FromArgb(28,25,35));e.Graphics.Clear(fill);using(var p=new Pen(Primary?fill:Theme.Line))e.Graphics.DrawRectangle(p,0,0,Width-1,Height-1);Theme.Text(e.Graphics,Text,10,Primary?Color.White:Theme.Ink,new Rectangle(14,0,Width-28,Height),FontStyle.Bold,TextFormatFlags.HorizontalCenter|TextFormatFlags.VerticalCenter);if(Focused)ControlPaint.DrawFocusRectangle(e.Graphics,new Rectangle(4,4,Width-8,Height-8));}
}
public class DisplayPicker:QuietButton {
 public int SelectedIndex;public string[] Items;
 public DisplayPicker(){AccessibleName="Capture display";}
 protected override void OnClick(EventArgs e){var menu=new ContextMenuStrip{BackColor=Theme.Paper,ForeColor=Theme.Ink,ShowImageMargin=false};for(int i=0;i<Items.Length;i++){int n=i;var item=menu.Items.Add(Items[i]);item.Click+=delegate{SelectedIndex=n;Text=Items[n]+"   v";};}menu.Closed+=delegate{menu.Dispose();};menu.Show(this,new Point(0,Height));base.OnClick(e);}
}
public class ModePicker:Control {
 public int SelectedIndex;public event EventHandler Changed;public ModePicker(){DoubleBuffered=true;TabStop=true;Cursor=Cursors.Hand;AccessibleName="Color mapping: per key, six zones, average. Use arrow keys.";}
 void Set(int i){SelectedIndex=Math.Max(0,Math.Min(2,i));Invalidate();if(Changed!=null)Changed(this,EventArgs.Empty);}
 protected override void OnMouseDown(MouseEventArgs e){Focus();Set(e.X*3/Width);base.OnMouseDown(e);}
 protected override bool IsInputKey(Keys k){return k==Keys.Left||k==Keys.Right||base.IsInputKey(k);}
 protected override void OnKeyDown(KeyEventArgs e){if(e.KeyCode==Keys.Left)Set(SelectedIndex-1);if(e.KeyCode==Keys.Right)Set(SelectedIndex+1);base.OnKeyDown(e);}
 protected override void OnPaint(PaintEventArgs e){e.Graphics.Clear(Color.FromArgb(26,23,33));int w=Width/3;using(var b=new SolidBrush(Theme.Accent))e.Graphics.FillRectangle(b,SelectedIndex*w+3,3,w-6,Height-6);string[] names={"Per key","6 zones","Average"};for(int i=0;i<3;i++)Theme.Text(e.Graphics,names[i],9,i==SelectedIndex?Color.White:Theme.Muted,new Rectangle(i*w,0,w,Height),FontStyle.Regular,TextFormatFlags.HorizontalCenter|TextFormatFlags.VerticalCenter);if(Focused)ControlPaint.DrawFocusRectangle(e.Graphics,ClientRectangle);}
}
public class FineSlider:Control {
 int value;public int Maximum=100;public event EventHandler ValueChanged;
 public int Value {get{return value;}set{this.value=Math.Max(0,Math.Min(Maximum,value));Invalidate();if(ValueChanged!=null)ValueChanged(this,EventArgs.Empty);}}
 public FineSlider(){DoubleBuffered=true;TabStop=true;Cursor=Cursors.Hand;}
 void SetValue(int x){Value=(int)Math.Round((x-7)*Maximum/(double)(Width-14));}
 protected override void OnMouseDown(MouseEventArgs e){Focus();Capture=true;SetValue(e.X);base.OnMouseDown(e);}protected override void OnMouseMove(MouseEventArgs e){if(Capture)SetValue(e.X);base.OnMouseMove(e);}protected override void OnMouseUp(MouseEventArgs e){Capture=false;base.OnMouseUp(e);}
 protected override bool IsInputKey(Keys k){return k==Keys.Left||k==Keys.Right||base.IsInputKey(k);}protected override void OnKeyDown(KeyEventArgs e){if(e.KeyCode==Keys.Left)Value-=5;if(e.KeyCode==Keys.Right)Value+=5;base.OnKeyDown(e);}
 protected override void OnPaint(PaintEventArgs e){e.Graphics.SmoothingMode=SmoothingMode.AntiAlias;int y=Height/2,x=7+(Width-14)*Value/Maximum;using(var p=new Pen(Theme.Line,3))e.Graphics.DrawLine(p,7,y,Width-7,y);using(var p=new Pen(Theme.Ink,3))e.Graphics.DrawLine(p,7,y,x,y);using(var b=new SolidBrush(Theme.Paper))e.Graphics.FillEllipse(b,x-7,y-7,14,14);using(var p=new Pen(Theme.Ink,2))e.Graphics.DrawEllipse(p,x-6,y-6,12,12);if(Focused)ControlPaint.DrawFocusRectangle(e.Graphics,ClientRectangle);}
}
public class OutputToggle:CheckBox {
 public OutputToggle(){SetStyle(ControlStyles.UserPaint|ControlStyles.AllPaintingInWmPaint|ControlStyles.OptimizedDoubleBuffer,true);Cursor=Cursors.Hand;AccessibleName="Send colors to keyboard. Experimental USB output";}
 protected override void OnPaint(PaintEventArgs e){e.Graphics.Clear(BackColor);e.Graphics.SmoothingMode=SmoothingMode.AntiAlias;using(var p=Theme.Round(new RectangleF(0,8,36,20),10))using(var b=new SolidBrush(Checked?Theme.Accent:Color.FromArgb(66,58,80)))e.Graphics.FillPath(b,p);using(var b=new SolidBrush(Color.White))e.Graphics.FillEllipse(b,Checked?19:3,11,14,14);Theme.Text(e.Graphics,Text.Length==0?"Send to keyboard":Text,10,Theme.Ink,new Rectangle(48,0,Width-48,36));if(Focused)ControlPaint.DrawFocusRectangle(e.Graphics,ClientRectangle);}
}
public class Preview:Control {
 public bool Running;public int Mapping;public Color[] Colors=IdleColors();static Color[] IdleColors(){var a=new Color[KeyboardLayout.Keys.Length];for(int i=0;i<a.Length;i++){var k=KeyboardLayout.Keys[i];a[i]=Color.FromArgb(90+k.X/5,55+(220-k.Y)/4,190+k.Y/4);}return a;}
 public Preview(){DoubleBuffered=true;AccessibleName="Live keyboard color preview";}
 protected override void OnPaint(PaintEventArgs e){var g=e.Graphics;g.Clear(Color.FromArgb(22,19,29));g.SmoothingMode=SmoothingMode.AntiAlias;
  Theme.Text(g,Mapping==0?"SCREEN MAP / PER KEY":Mapping==1?"SCREEN MAP / 6 ZONES":"SCREEN MAP / AVERAGE",8,Theme.Muted,new Rectangle(24,18,300,26),FontStyle.Bold);
  Theme.Text(g,Running?"LIVE":"PREVIEW",8,Running?Color.FromArgb(184,151,255):Theme.Muted,new Rectangle(Width-100,18,76,26),FontStyle.Bold,TextFormatFlags.Right|TextFormatFlags.VerticalCenter);
  for(int n=0;n<KeyboardLayout.Keys.Length;n++){var k=KeyboardLayout.Keys[n];Rectangle r=ScreenSampler.Region(k,Width-48,58);r.Offset(24,56);using(var brush=new SolidBrush(Colors[n]))g.FillRectangle(brush,r.X,r.Y,Math.Max(1,r.Width-1),Math.Max(1,r.Height-1));}
  float scale=(Width-80)/650f;var save=g.Save();g.TranslateTransform(40,125);g.ScaleTransform(scale,scale);
  using(var path=Theme.Round(new RectangleF(0,0,650,244),12))using(var b=new SolidBrush(Color.FromArgb(10,9,14)))g.FillPath(b,path);
  for(int n=0;n<KeyboardLayout.Keys.Length;n++){var k=KeyboardLayout.Keys[n];Color c=Colors[n];var r=new RectangleF(k.X,k.Y,k.W,k.H);using(var path=Theme.Round(r,3)){using(var b=new SolidBrush(Color.FromArgb(37,32,46)))g.FillPath(b,path);using(var p=new Pen(Color.FromArgb(80,c),1))g.DrawPath(p,path);}using(var b=new SolidBrush(c))g.FillRectangle(b,k.X+5,k.Y+k.H-3,k.W-10,2);
   string name=k.Name;using(var font=Theme.Type(7.5f))using(var brush=new SolidBrush(Color.FromArgb(230,222,245)))using(var sf=new StringFormat{Alignment=StringAlignment.Center,LineAlignment=StringAlignment.Center,FormatFlags=StringFormatFlags.NoWrap})g.DrawString(name,font,brush,new RectangleF(k.X,k.Y-1,k.W,k.H),sf);
  }
  using(var b=new SolidBrush(Color.FromArgb(50,42,63)))g.FillEllipse(b,601,28,29,29);using(var p=new Pen(Color.FromArgb(157,139,184),2))g.DrawLine(p,615,31,615,38);g.Restore(save);
  Theme.Text(g,"HIVE 65",9,Theme.Ink,new Rectangle(24,Height-48,200,25),FontStyle.Bold);
  Theme.Text(g,Running?"Colors follow your display":"Start capture to see your screen here",8,Theme.Muted,new Rectangle(180,Height-48,Width-204,25),FontStyle.Regular,TextFormatFlags.Right|TextFormatFlags.VerticalCenter);
 }
}
public class MainForm:Form {
 DisplayPicker monitor=new DisplayPicker();ModePicker mode=new ModePicker();FineSlider brightness=new FineSlider(),smooth=new FineSlider();OutputToggle usb=new OutputToggle(),vivid=new OutputToggle();QuietButton start=new QuietButton();Label status=new Label(),brightnessValue=new Label(),smoothValue=new Label();Preview preview=new Preview();System.Windows.Forms.Timer timer=new System.Windows.Forms.Timer();bool active,busy;int generation;HidDevice device;
 public MainForm(){Text="Hive Screen RGB";ClientSize=new Size(1000,654);FormBorderStyle=FormBorderStyle.FixedSingle;MaximizeBox=false;BackColor=Theme.Paper;ForeColor=Theme.Ink;Font=Theme.Type(10);StartPosition=FormStartPosition.CenterScreen;AutoScaleMode=AutoScaleMode.None;DoubleBuffered=true;
  LabelText("hive",30,22,130,45,27,Theme.Ink,FontStyle.Bold);LabelText("Screen RGB",190,31,200,30,12,Theme.Muted);LabelText("HIVE 65  /  USB",760,34,210,24,9,Theme.Muted,FontStyle.Bold,ContentAlignment.MiddleRight);
  LabelText("Your screen, in color.",30,96,610,48,21,Theme.Ink);LabelText("Ambient lighting for the keys in front of you.",30,150,600,28,10,Theme.Muted);
  preview.SetBounds(30,192,626,386);Controls.Add(preview);
  LabelText("CAPTURE SETTINGS",700,106,270,24,8,Theme.Muted,FontStyle.Bold);
  LabelText("Display",700,148,270,24,10,Theme.Ink,FontStyle.Bold);monitor.SetBounds(700,181,270,40);var displays=new List<string>();int d=1;foreach(var screen in Screen.AllScreens)displays.Add("Display "+d+++"  /  "+screen.Bounds.Width+" x "+screen.Bounds.Height);monitor.Items=displays.ToArray();monitor.Text=monitor.Items[0]+"   v";Controls.Add(monitor);
  LabelText("Color mapping",700,244,270,24,10,Theme.Ink,FontStyle.Bold);mode.SetBounds(700,277,270,38);Controls.Add(mode);mode.Changed+=delegate{preview.Mapping=mode.SelectedIndex;preview.Invalidate();};
  LabelText("Brightness",700,340,180,24,10,Theme.Ink,FontStyle.Bold);SetupValue(brightnessValue,"75%",910,340);brightness.SetBounds(700,370,270,26);brightness.Value=75;brightness.AccessibleName="Brightness";brightness.ValueChanged+=delegate{brightnessValue.Text=brightness.Value+"%";};Controls.Add(brightness);
  LabelText("Smoothing",700,418,180,24,10,Theme.Ink,FontStyle.Bold);SetupValue(smoothValue,"55%",910,418);smooth.SetBounds(700,448,270,26);smooth.Maximum=90;smooth.Value=55;smooth.AccessibleName="Smoothing";smooth.ValueChanged+=delegate{smoothValue.Text=smooth.Value+"%";};Controls.Add(smooth);vivid.Text="Vivid colors";vivid.Checked=true;vivid.AccessibleName="Vivid colors: favor saturated screen pixels";vivid.SetBounds(700,480,270,36);Controls.Add(vivid);
  usb.SetBounds(700,526,270,36);usb.CheckedChanged+=delegate{if(active)Stop();start.Text=usb.Checked?"Start RGB sync":"Start preview";};Controls.Add(usb);LabelText("USB output is experimental.",748,562,222,20,8,Theme.Muted);
  status.SetBounds(30,609,620,24);status.Font=Theme.Type(9);status.ForeColor=Theme.Muted;status.Text="Ready when you are. No keyboard needed for preview.";status.AutoEllipsis=true;Controls.Add(status);
  start.Name="start";start.Text="Start preview";start.Primary=true;start.SetBounds(700,603,270,38);start.Click+=Toggle;Controls.Add(start);
  timer.Interval=67;timer.Tick+=Tick;FormClosing+=delegate{Stop();};
 }
 [DllImport("dwmapi.dll")] static extern int DwmSetWindowAttribute(IntPtr hwnd,int attribute,ref int value,int size);
 protected override void OnHandleCreated(EventArgs e){base.OnHandleCreated(e);try{int dark=1;DwmSetWindowAttribute(Handle,20,ref dark,4);}catch(DllNotFoundException){}catch(EntryPointNotFoundException){}}
 protected override void OnPaint(PaintEventArgs e){base.OnPaint(e);using(var p=new Pen(Theme.Line)){e.Graphics.DrawLine(p,30,83,970,83);e.Graphics.DrawLine(p,678,105,678,578);e.Graphics.DrawLine(p,30,593,970,593);}}
 void LabelText(string text,int x,int y,int w,int h,float size,Color color,FontStyle style=FontStyle.Regular,ContentAlignment align=ContentAlignment.MiddleLeft){Controls.Add(new Label{Text=text,Bounds=new Rectangle(x,y,w,h),Font=Theme.Type(size,style),ForeColor=color,TextAlign=align});}
 void SetupValue(Label l,string text,int x,int y){l.Text=text;l.SetBounds(x,y,60,24);l.TextAlign=ContentAlignment.MiddleRight;l.Font=Theme.Type(10);l.ForeColor=Theme.Muted;Controls.Add(l);}
 void Stop(){generation++;bool wasUsb=device!=null;active=false;preview.Running=false;preview.Invalidate();timer.Stop();if(device!=null){device.Dispose();device=null;}start.Text=usb.Checked?"Start RGB sync":"Start preview";status.Text=wasUsb?"Stopped. Restore your lighting effect with the Kreo app.":"Capture paused. Start again whenever you're ready.";}
 void Toggle(object sender,EventArgs e){if(active){Stop();return;}try{if(usb.Checked)device=HidDevice.Connect();generation++;active=true;preview.Running=true;preview.Invalidate();start.Text="Stop";timer.Start();status.Text=usb.Checked?"Sending screen colors to your keyboard.":"Capturing your display. Preview only.";}catch(Exception ex){Stop();status.Text=ex.Message;MessageBox.Show(this,ex.Message,"Keyboard connection",MessageBoxButtons.OK,MessageBoxIcon.Information);}}
 async void Tick(object sender,EventArgs e){if(busy||!active)return;busy=true;try{
   int ix=Math.Min(monitor.SelectedIndex,Screen.AllScreens.Length-1);Rectangle rect=Screen.AllScreens[ix].Bounds;int mapping=mode.SelectedIndex,run=generation;bool vibrant=vivid.Checked;double b=brightness.Value/100.0,a=1-smooth.Value/100.0;
   Color[] colors=await Task.Run(()=>ScreenSampler.Sample(rect,mapping,vibrant));if(!active||IsDisposed||run!=generation)return;
   for(int i=0;i<colors.Length;i++)colors[i]=ScreenSampler.Blend(preview.Colors[i],colors[i],a,b);preview.Colors=colors;preview.Invalidate();if(device!=null)device.Send(colors);
  }catch(Exception ex){Stop();status.Text="Paused: "+ex.Message;}finally{busy=false;}}
}
public static class Program {
 [DllImport("user32.dll")] static extern bool SetProcessDPIAware();
 [STAThread] public static void Main(string[] args){SetProcessDPIAware();Application.EnableVisualStyles();Application.SetCompatibleTextRenderingDefault(false);if(args.Length>0&&args[0]=="--self-test"){SelfTest(args.Length>1?args[1]:"self-test.txt");return;}Application.Run(new MainForm());}
 static void Check(bool value,string message){if(!value)throw new Exception(message);}
 static void SelfTest(string path){var log=new List<string>();try{
  var known=new Color[]{Color.Red,Color.Lime,Color.Blue,Color.White,Color.Black,Color.Yellow};
  using(var b=new Bitmap(120,60)){using(var g=Graphics.FromImage(b))for(int i=0;i<6;i++)using(var brush=new SolidBrush(known[i]))g.FillRectangle(brush,i*20,0,20,60);var actual=ScreenSampler.Average(b);for(int i=0;i<6;i++)Check(actual[i].ToArgb()==known[i].ToArgb(),"zone sampling");}log.Add("PASS: six-zone image sampling with known RGB stripes");
  var perKey=new Color[KeyboardLayout.Keys.Length];for(int n=0;n<perKey.Length;n++)perKey[n]=Color.FromArgb(n*3,n*2,255-n*3);var packet=Protocol.Frame(perKey);Check(packet.Length==520&&packet[0]==6&&packet[1]==8&&packet[4]==1&&packet[5]==0&&packet[6]==0x7a&&packet[7]==1,"report header");
  var seen=new HashSet<int>();for(int n=0;n<KeyboardLayout.Keys.Length;n++){var k=KeyboardLayout.Keys[n];Check(seen.Add(k.Id),"duplicate key");int i=8+3*k.Id;var c=perKey[n];Check(packet[i]==c.R&&packet[i+1]==c.G&&packet[i+2]==c.B,"key RGB");}Check(seen.Count==67,"visible key coverage");
  for(int id=0;id<126;id++)if(!seen.Contains(id))Check(packet[8+3*id]==0&&packet[9+3*id]==0&&packet[10+3*id]==0,"unused slot");
  for(int i=386;i<520;i++)Check(packet[i]==0,"padding");log.Add("PASS: 520-byte report, 378-byte RGB matrix, 67 key slots, unused slots and padding");

  Check(ScreenSampler.Blend(Color.Black,Color.White,1,0).R==0,"brightness zero");Check(ScreenSampler.Blend(Color.Black,Color.White,0.5,1).R==128,"smoothing");log.Add("PASS: brightness and smoothing");
  using(var test=new Bitmap(192,108)){
   using(var g=Graphics.FromImage(test))g.Clear(Color.Black);foreach(var c in ScreenSampler.Map(test,0,true))Check(c.ToArgb()==Color.Black.ToArgb(),"black preservation");
   using(var g=Graphics.FromImage(test))g.Clear(Color.White);foreach(var c in ScreenSampler.Map(test,0,true))Check(c.ToArgb()==Color.White.ToArgb(),"white neutrality");
   for(int y=0;y<test.Height;y++)for(int x=0;x<test.Width;x++)test.SetPixel(x,y,Color.FromArgb(x*255/191,y*255/107,40));
   var mapped=ScreenSampler.Map(test,0,false);int esc=0,ctrl=0,right=0;for(int n=0;n<KeyboardLayout.Keys.Length;n++){var k=KeyboardLayout.Keys[n];if(k.Id==1)esc=n;if(k.Id==5)ctrl=n;if(k.Id==95)right=n;Rectangle r=ScreenSampler.Region(k,192,108);Check(r.Left>=0&&r.Top>=0&&r.Right<=192&&r.Bottom<=108,"mapping bounds");}
   Check(mapped[ctrl].G>mapped[esc].G+120,"vertical mapping");Check(mapped[right].R>mapped[ctrl].R+160,"horizontal mapping");
   var zones=ScreenSampler.Map(test,1,false);Check(zones[ctrl].ToArgb()==zones[esc].ToArgb(),"six-zone consistency");var average=ScreenSampler.Map(test,2,false);foreach(var c in average)Check(c.ToArgb()==average[0].ToArgb(),"average uniformity");
   using(var g=Graphics.FromImage(test)){g.Clear(Color.FromArgb(100,100,100));using(var brush=new SolidBrush(Color.Red))g.FillRectangle(brush,0,0,96,108);}
   var natural=ScreenSampler.Area(test,new Rectangle(0,0,192,108),false);var colorful=ScreenSampler.Area(test,new Rectangle(0,0,192,108),true);Check(colorful.R>natural.R&&colorful.G<natural.G,"vivid color weighting");
  }log.Add("PASS: 2D per-key mapping, map bounds, optional modes, black/white preservation, vivid weighting");
  Color[] desktop=ScreenSampler.Sample(Screen.PrimaryScreen.Bounds);Check(desktop.Length==67,"desktop capture");log.Add("PASS: live primary-display capture (pixels not saved)");
  using(var f=new MainForm()){f.Show();Application.DoEvents();using(var shot=new Bitmap(f.Width,f.Height)){f.DrawToBitmap(shot,new Rectangle(0,0,f.Width,f.Height));shot.Save(Path.Combine(Path.GetDirectoryName(path),"redesign-idle.png"));}foreach(Control ctrl in f.Controls){var combo=ctrl as ComboBox;if(combo!=null){Check(combo.SelectedIndex>=0,"selection missing");log.Add("Selected: "+combo.Text);}} foreach(Control ctrl in f.Controls){var btn=ctrl as Button;if(btn!=null && btn.Name=="start")btn.PerformClick();} var watch=System.Diagnostics.Stopwatch.StartNew();while(watch.ElapsedMilliseconds<450){Application.DoEvents();System.Threading.Thread.Sleep(10);} foreach(Control ctrl in f.Controls){var btn=ctrl as Button;if(btn!=null && btn.Name=="start"){Check(btn.Text=="Stop","preview loop stopped");btn.PerformClick();}}log.Add("PASS: start, live preview timer and stop");using(var b=new Bitmap(f.Width,f.Height)){f.DrawToBitmap(b,new Rectangle(0,0,f.Width,f.Height));b.Save(Path.ChangeExtension(path,"png"));}f.Close();}log.Add("PASS: Windows UI created and rendered");
  log.Add("Hardware output NOT TESTED: no keyboard available.");File.WriteAllLines(path,log);
 }catch(Exception ex){log.Add("FAIL: "+ex);File.WriteAllLines(path,log);Environment.ExitCode=1;}}
}






