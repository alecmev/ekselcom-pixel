using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Collections.ObjectModel;

namespace MyFa
{
    #region ENums
    public enum DrawingTool
    {
        Pencil,
        Line,
        Fill,
        Symbol
    }

    public enum DrawingMode
    {
        Draw,
        Erase
    }
    #endregion

    public partial class SymbolEditWindow : Window
    {
        #region Variables & Properties
        private SymbolStorage symbol;
        public SymbolStorage Symbol
        {
            get { return symbol; }
            private set
            {
                symbol = value;
                UpdateBoard();
            }
        }

        private Boolean updating = false, undoredo = false;
        private Rectangle[][] rectangles;

        private DrawingTool Tool = DrawingTool.Pencil;
        private Boolean[] ToolMouseDown;

        private DrawingMode Mode = DrawingMode.Draw;
        private Boolean[] ModeMouseDown;

        private Double PrevX = -1, PrevY = -1;

        private Stack<Stack<UndoRedo>> Undo, Redo;

        public SymbolPickerWindow spw;
        #endregion

        #region Initialization
        public SymbolEditWindow(SymbolStorage newSymbol, SymbolPickerWindow newSpw)
        {
            InitializeComponent();
            ToolMouseDown = new Boolean[sizeof(DrawingTool)];
            ModeMouseDown = new Boolean[sizeof(DrawingMode)];
            Symbol = new SymbolStorage(newSymbol);
            spw = newSpw;

            iSize.Title = "Size " + Symbol.Width + "x" + Symbol.Height;
            iDec.Title = "Dec " + Symbol.Code;
            iHex.Title = "Hex 0x" + (Symbol.Code / 16).ToString() + (Symbol.Code % 16).ToString();
            iAscii.Title = "Ascii " + Symbol.Ascii;
            iSymbol.Title = "Symbol '" + (System.Text.Encoding.GetEncoding(Symbol.Ascii).GetChars(new byte[] { Symbol.Code }))[0].ToString() + "'";
            iFont.Title = "Font " + Symbol.Font;
            iFontSize.Title = "Font Size " + Symbol.Size.ToString("F");
        }

        private void UpdateBoard()
        {
            updating = true;

            Line tmpLine;
            Board.Children.Clear();
            Board.Width = Symbol.Width;
            Board.Height = Symbol.Height;
            Scope.Value = 32;

            rectangles = new Rectangle[Symbol.Height][];
            for (int i = 0; i < Symbol.Height; ++i) rectangles[i] = new Rectangle[Symbol.Width];

            if (Symbol.Map[0][0]) DrawPixel(0, 0);

            for (int i = 1; i < Symbol.Width; ++i)
            {
                tmpLine = new Line();
                tmpLine.X1 = i;
                tmpLine.Y1 = 0;
                tmpLine.X2 = i;
                tmpLine.Y2 = Symbol.Height;
                tmpLine.StrokeThickness = 0.05;
                tmpLine.Stroke = Brushes.Gray;
                Board.Children.Add(tmpLine);

                if (Symbol.Map[0][i]) DrawPixel(i, 0);
            }

            for (int i = 1; i < Symbol.Height; ++i)
            {
                tmpLine = new Line();
                tmpLine.X1 = 0;
                tmpLine.Y1 = i;
                tmpLine.X2 = Symbol.Width;
                tmpLine.Y2 = i;
                tmpLine.StrokeThickness = 0.05;
                tmpLine.Stroke = Brushes.Gray;
                Board.Children.Add(tmpLine);

                for (int j = 0; j < Symbol.Width; ++j)
                {
                    if (Symbol.Map[i][j]) DrawPixel(j, i);
                }
            }

            Undo = new Stack<Stack<UndoRedo>>();
            Redo = new Stack<Stack<UndoRedo>>();

            updating = false;
        }
        #endregion

        #region Event Handlers
        private void Scope_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            Scope.Value = 32;
        }

        private void Board_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed || e.RightButton == MouseButtonState.Pressed)
            {
                PrevX = e.GetPosition((Canvas)sender).X;
                PrevY = e.GetPosition((Canvas)sender).Y;
                if (Undo.Count == 0 || Undo.First().Count > 0) Undo.Push(new Stack<UndoRedo>());
                Redo.Clear();
                if (Tool == DrawingTool.Pencil)
                {
                    if (e.RightButton == MouseButtonState.Pressed) InvertMode();
                    DrawLine(PrevX, PrevY, PrevX, PrevY);
                    if (e.RightButton == MouseButtonState.Pressed) InvertMode();
                }
            }
        }

        private void Board_MouseMove(object sender, MouseEventArgs e)
        {
            iX.Title = "X " + Math.Floor(e.GetPosition((Canvas)sender).X);
            iY.Title = "Y " + Math.Floor(e.GetPosition((Canvas)sender).Y);
            if (PrevX > -1 && Tool == DrawingTool.Pencil && (e.LeftButton == MouseButtonState.Pressed || e.RightButton == MouseButtonState.Pressed))
            {
                if (e.RightButton == MouseButtonState.Pressed) InvertMode();
                DrawLine(PrevX, PrevY, e.GetPosition((Canvas)sender).X, e.GetPosition((Canvas)sender).Y);
                if (e.RightButton == MouseButtonState.Pressed) InvertMode();
                PrevX = e.GetPosition((Canvas)sender).X;
                PrevY = e.GetPosition((Canvas)sender).Y;
            }
        }

        private void Board_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (PrevX > -1 && (e.ChangedButton == MouseButton.Left || e.ChangedButton == MouseButton.Right))
            {
                if (e.ChangedButton == MouseButton.Right) InvertMode();
                if (Tool == DrawingTool.Line) DrawLine(PrevX, PrevY, e.GetPosition((Canvas)sender).X, e.GetPosition((Canvas)sender).Y);
                else if (Tool == DrawingTool.Fill) Fill(e.GetPosition((Canvas)sender).X, e.GetPosition((Canvas)sender).Y);
                if (e.ChangedButton == MouseButton.Right) InvertMode();

                PrevX = -1;
                PrevY = -1;
            }
        }

        private void Board_MouseEnter(object sender, MouseEventArgs e)
        {
            if (PrevX > -1 && (e.LeftButton == MouseButtonState.Pressed || e.RightButton == MouseButtonState.Pressed) && Tool != DrawingTool.Line)
            {
                PrevX = e.GetPosition((Canvas)sender).X;
                PrevY = e.GetPosition((Canvas)sender).Y;
            }
        }

        private void Board_MouseLeave(object sender, MouseEventArgs e)
        {
            if ((e.LeftButton == MouseButtonState.Released && e.RightButton == MouseButtonState.Released) || Tool != DrawingTool.Line)
            {
                if (PrevX > -1 && (e.LeftButton == MouseButtonState.Pressed || e.RightButton == MouseButtonState.Pressed))
                {
                    PrevX = e.GetPosition((Canvas)sender).X;
                    PrevY = e.GetPosition((Canvas)sender).Y;
                }
                else
                {
                    PrevX = -1;
                    PrevY = -1;
                }
            }
        }

        private void UndoClick(object sender, RoutedEventArgs e)
        {
            if (Undo.Count > 0)
            {
                undoredo = true;
                Stack<UndoRedo> tmpUndo = Undo.Pop();
                Redo.Push(new Stack<UndoRedo>());
                Stack<UndoRedo> tmpRedo = Redo.First();
                UndoRedo tmpUndoRedo;
                DrawingMode oldMode = Mode;

                while (tmpUndo.Count > 0)
                {
                    tmpUndoRedo = tmpUndo.Pop();

                    Mode = tmpUndoRedo.Mode;
                    DrawPixel(tmpUndoRedo.X, tmpUndoRedo.Y);

                    tmpUndoRedo.InvertMode();
                    tmpRedo.Push(tmpUndoRedo);
                }

                Mode = oldMode;
                undoredo = false;
            }
        }

        private void RedoClick(object sender, RoutedEventArgs e)
        {
            if (Redo.Count > 0)
            {
                undoredo = true;
                Stack<UndoRedo> tmpRedo = Redo.Pop();
                Undo.Push(new Stack<UndoRedo>());
                Stack<UndoRedo> tmpUndo = Undo.First();
                UndoRedo tmpUndoRedo;
                DrawingMode oldMode = Mode;

                while (tmpRedo.Count > 0)
                {
                    tmpUndoRedo = tmpRedo.Pop();

                    Mode = tmpUndoRedo.Mode;
                    DrawPixel(tmpUndoRedo.X, tmpUndoRedo.Y);

                    tmpUndoRedo.InvertMode();
                    tmpUndo.Push(tmpUndoRedo);
                }

                Mode = oldMode;
                undoredo = false;
            }
        }

        private void Window_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Z && Keyboard.Modifiers == ModifierKeys.Control && Undo.Count > 0) UndoClick(sender, null);
            else if (e.Key == Key.Y && Keyboard.Modifiers == ModifierKeys.Control && Redo.Count > 0) RedoClick(sender, null);
        }

        private void SaveClick(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }

        private void CancelClick(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }

        private void MoveSymbol(object sender, RoutedEventArgs e)
        {
            if (Undo.Count == 0 || Undo.First().Count > 0) Undo.Push(new Stack<UndoRedo>());
            Redo.Clear();
            DrawingMode old = Mode;
            if (((SecondMenuButton)sender).Title == "←")
            {
                for (int i = 0; i < Symbol.Height; ++i)
                {
                    for (int j = 0; j < Symbol.Width - 1; ++j)
                    {
                        Mode = Symbol.Map[i][j + 1] ? DrawingMode.Draw : DrawingMode.Erase;
                        DrawPixel(j, i);
                    }
                }
                Mode = DrawingMode.Erase;
                for (int i = 0; i < Symbol.Height; ++i) DrawPixel(Symbol.Width - 1, i);
            }
            else if (((SecondMenuButton)sender).Title == "↑")
            {
                for (int i = 0; i < Symbol.Height - 1; ++i)
                {
                    for (int j = 0; j < Symbol.Width; ++j)
                    {
                        Mode = Symbol.Map[i + 1][j] ? DrawingMode.Draw : DrawingMode.Erase;
                        DrawPixel(j, i);
                    }
                }
                Mode = DrawingMode.Erase;
                for (int j = 0; j < Symbol.Width; ++j) DrawPixel(j, Symbol.Height - 1);
            }
            else if (((SecondMenuButton)sender).Title == "→")
            {
                for (int i = 0; i < Symbol.Height; ++i)
                {
                    for (int j = Symbol.Width - 1; j > 0; --j)
                    {
                        Mode = Symbol.Map[i][j - 1] ? DrawingMode.Draw : DrawingMode.Erase;
                        DrawPixel(j, i);
                    }
                }
                Mode = DrawingMode.Erase;
                for (int i = 0; i < Symbol.Height; ++i) DrawPixel(0, i);
            }
            else
            {
                for (int i = Symbol.Height - 1; i > 0; --i)
                {
                    for (int j = 0; j < Symbol.Width; ++j)
                    {
                        Mode = Symbol.Map[i - 1][j] ? DrawingMode.Draw : DrawingMode.Erase;
                        DrawPixel(j, i);
                    }
                }
                Mode = DrawingMode.Erase;
                for (int j = 0; j < Symbol.Width; ++j) DrawPixel(j, 0);
            }
            Mode = old;
        }
        #endregion

        #region Methods
        private void InvertMode()
        {
            if (Mode == DrawingMode.Draw) Mode = DrawingMode.Erase;
            else Mode = DrawingMode.Draw;
        }

        private void DrawPixel(Int32 X, Int32 Y)
        {
            if ((Mode == DrawingMode.Draw && !Symbol.Map[Y][X]) || updating)
            {
                if (rectangles[Y][X] == null)
                {
                    rectangles[Y][X] = new Rectangle();
                    rectangles[Y][X].Width = 1;
                    rectangles[Y][X].Height = 1;
                    rectangles[Y][X].Fill = Brushes.Black;
                }
                Canvas.SetLeft(rectangles[Y][X], X);
                Canvas.SetTop(rectangles[Y][X], Y);
                Symbol.Map[Y][X] = true;
                Board.Children.Insert(0, rectangles[Y][X]);
                if (!updating && !undoredo) Undo.First().Push(new UndoRedo(X, Y, DrawingMode.Erase));
            }
            else if (Mode == DrawingMode.Erase && Symbol.Map[Y][X])
            {
                Symbol.Map[Y][X] = false;
                Board.Children.Remove(rectangles[Y][X]);
                if (!updating && !undoredo) Undo.First().Push(new UndoRedo(X, Y, DrawingMode.Draw));
            }
        }

        private void DrawLine(Double X1, Double Y1, Double X2, Double Y2)
        {
            Int32 FX1 = (Int32)Math.Floor(X1);
            if (FX1 == Symbol.Width) --FX1;
            Int32 FY1 = (Int32)Math.Floor(Y1);
            if (FY1 == Symbol.Height) --FY1;
            Int32 FX2 = (Int32)Math.Floor(X2);
            if (FX2 == Symbol.Width) --FX2;
            Int32 FY2 = (Int32)Math.Floor(Y2);
            if (FY2 == Symbol.Height) --FY2;

            if (FX1 == FX2 && FY1 == FY2)
            {
                DrawPixel(FX1, FY1);
            }
            else if (Math.Abs(FX2 - FX1) > Math.Abs(FY2 - FY1))
            {
                if (FX2 < FX1)
                {
                    FX1 += FX2;
                    FX2 = FX1 - FX2;
                    FX1 -= FX2;
                    FY1 += FY2;
                    FY2 = FY1 - FY2;
                    FY1 -= FY2;
                }

                Int32 FX = FX2 - FX1;
                Int32 FY = FY2 - FY1;

                for (Int32 x = 0; x <= FX; ++x) DrawPixel(FX1 + x, (Int32)((Double)FY1 + ((Double)FY * (Double)x) / (Double)FX));
            }
            else
            {
                if (FY2 < FY1)
                {
                    FX1 += FX2;
                    FX2 = FX1 - FX2;
                    FX1 -= FX2;
                    FY1 += FY2;
                    FY2 = FY1 - FY2;
                    FY1 -= FY2;
                }

                Int32 FX = FX2 - FX1;
                Int32 FY = FY2 - FY1;

                for (Int32 y = 0; y <= FY; ++y) DrawPixel((Int32)((Double)FX1 + ((Double)FX * (Double)y) / (Double)FY), FY1 + y);
            }
        }

        private void Fill(Double X, Double Y)
        {
            Int32 FX = (Int32)Math.Floor(X);
            if (FX == Symbol.Width) --FX;
            Int32 FY = (Int32)Math.Floor(Y);
            if (FY == Symbol.Height) --FY;
            Filler(FX, FY);
        }

        private void Filler(Int32 X, Int32 Y)
        {
            if ((Mode == DrawingMode.Draw && Symbol.Map[Y][X]) || (Mode == DrawingMode.Erase && !Symbol.Map[Y][X])) return;
            DrawPixel(X, Y);
            if (X > 0) Filler(X - 1, Y);
            if (Y > 0) Filler(X, Y - 1);
            if (X < Symbol.Width - 1) Filler(X + 1, Y);
            if (Y < Symbol.Height - 1) Filler(X, Y + 1);
        }
        #endregion

        #region Tool Selection
        private DrawingTool GetToolByButton(object bySender)
        {
            switch (((SecondMenuButton)bySender).Title)
            {
                case "pencil":
                    return DrawingTool.Pencil;
                case "line":
                    return DrawingTool.Line;
                case "fill":
                    return DrawingTool.Fill;
                case "symbol":
                    return DrawingTool.Symbol;
            }
            return DrawingTool.Pencil;
        }

        private SecondMenuButton GetButtonByTool(DrawingTool byTool)
        {
            switch (byTool)
            {
                case DrawingTool.Pencil:
                    return toolPencil;
                case DrawingTool.Line:
                    return toolLine;
                case DrawingTool.Fill:
                    return toolFill;
                case DrawingTool.Symbol:
                    return toolSymbol;
            }
            return toolPencil;
        }

        private void Tool_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Array.Clear(ToolMouseDown, 0, ToolMouseDown.Length);
            ToolMouseDown[(int)GetToolByButton(sender)] = true;
        }

        private void Tool_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (ToolMouseDown[(int)GetToolByButton(sender)])
            {
                if (GetToolByButton(sender) == DrawingTool.Symbol)
                {
                    spw = new SymbolPickerWindow(symbol, spw);
                    spw.Owner = this;
                    if (spw.ShowDialog() == true)
                    {
                        symbol = spw.Symbol;
                        iFont.Title = "Font " + Symbol.Font;
                        iFontSize.Title = "Font Size " + Symbol.Size.ToString("F");
                        UpdateBoard();
                    }
                    spw.Close();
                }
                else
                {
                    GetButtonByTool(Tool).Selected = false;
                    Tool = GetToolByButton(sender);
                    ((SecondMenuButton)sender).Selected = true;
                    ToolMouseDown[(int)Tool] = false;
                }
            }
        }
        #endregion

        #region Mode Selection
        private DrawingMode GetModeByButton(object bySender)
        {
            switch (((SecondMenuButton)bySender).Title)
            {
                case "draw":
                    return DrawingMode.Draw;
                case "erase":
                    return DrawingMode.Erase;
            }
            return DrawingMode.Draw;
        }

        private SecondMenuButton GetButtonByMode(DrawingMode byMode)
        {
            switch (byMode)
            {
                case DrawingMode.Draw:
                    return modeDraw;
                case DrawingMode.Erase:
                    return modeErase;
            }
            return modeDraw;
        }

        private void Mode_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Array.Clear(ModeMouseDown, 0, ModeMouseDown.Length);
            ModeMouseDown[(int)GetModeByButton(sender)] = true;
        }

        private void Mode_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (ModeMouseDown[(int)GetModeByButton(sender)])
            {
                GetButtonByMode(Mode).Selected = false;
                Mode = GetModeByButton(sender);
                ((SecondMenuButton)sender).Selected = true;
                ModeMouseDown[(int)Mode] = false;
            }
        }
        #endregion
    }

    #region Classes
    public class UndoRedo
    {
        public Int32 X, Y;
        public DrawingMode Mode;

        public UndoRedo(Int32 newX, Int32 newY, DrawingMode newMode)
        {
            X = newX;
            Y = newY;
            Mode = newMode;
        }

        public void InvertMode()
        {
            if (Mode == DrawingMode.Draw) Mode = DrawingMode.Erase;
            else Mode = DrawingMode.Draw;
        }
    } 
    #endregion
}
