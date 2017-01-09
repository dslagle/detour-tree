/***************************************************************************************************************************
 * SubrouteSelectorForm.cs
 * 
 * 
 * Date     Who Description
 * ======== === ===========================================================================================================
 * 04-23-15 DJS Header Added
 * 04-23-15 DJS DM#93109 DEVCASE: Visual Headways > Subroute selection form should group subroutes by master route
 * 03-08-16 DJS DM#96295 - Visual Headways Displays Different Vehicles In the Active Tab With The Active Checkbox Selected For All Users
 * 03-11-16 DJS DM#96563 - DEVCASE: FR Dispatching > Update custom forms to be resizable
 * 04-04-16 DJS DM#96831 - Visual Headways > Only active subroutes should be selected when selecting a master route while the active filter is on
**************************************************************************************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Drawing;
using System.Windows.Forms;

using RM = RouteMatch.Common.UI.CommonControls.Generic;
using RouteMatch.CA.DayOfDetour.Model;
using System.ComponentModel;

namespace DetourList
{
    /// <summary>
    /// Control to select subroutes grouped by their master route
    /// </summary>
    class DetourSelectionList : Control
    {
        private List<IListItem> m_MasterRouteItems = new List<IListItem>();
        private IListItem _HotItem = null;

        //height of an item in the list
        private const int ITEM_HEIGHT = 18;
        private const int PADDING = 3;
        //message the is displayed when no items are in the list (editable from the designer)
        private string m_EmptyMessage = "No Items to Display";

        //scrollbar used when more items are displayed than fit in the viewable area
        private RM.ScrollBar m_Scroll = new RM.ScrollBar();
        //current scroll position
        private int m_ScrollPosition = 0;

        //true if the scroll bar should be shown
        private bool m_ScrollVisible = false;
        private DetourSelectionListStyle _Style = new DetourSelectionListStyle();
        
        /// <summary>
        /// Message displayed when there are no items in the list
        /// </summary>
        public string EmptyMessage { get { return m_EmptyMessage; } set { m_EmptyMessage = value; Invalidate(); } }

        [Browsable(true)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        public DetourSelectionListStyle Style { get { return _Style; } }

        private Detour _Model;
        private Action<IListItem> ItemInvalidated;

        /// <summary>
        /// Create a new instance of the subroute selection list
        /// </summary>
        public DetourSelectionList()
        {
            ItemInvalidated = (item) => { Invalidate(item.Bounds); };
            Style.Invalidate += () => Invalidate();

            //listen for scroll events from the scrollbar
            m_Scroll.Scroll += (i) => { HandleScroll(i); };

            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.UserPaint | ControlStyles.ResizeRedraw, true);
            SetStyle(ControlStyles.StandardDoubleClick, false);
        }

        public void RenderModel(Detour model)
        {
            _Model = model;
            BuildVisualModel(model);
            Invalidate();
        }

        private void BuildVisualModel(Detour model)
        {
            m_MasterRouteItems.Clear();

            foreach (var mr in model.MasterRoutes)
            {
                var mrItem = new MasterRouteListItem(mr);
                mrItem.Invalidate += ItemInvalidated;

                foreach (var sr in mr.Subroutes)
                {
                    var srItem = new SubrouteListItem(sr) { Parent = mrItem };
                    srItem.Invalidate += ItemInvalidated;

                    foreach (var stop in sr.Stops)
                    {
                        var stopItem = new StopListItem(stop) { Parent = srItem };
                        stopItem.Invalidate += ItemInvalidated;
                        srItem.Children.Add(stopItem);
                    }

                    mrItem.Children.Add(srItem);
                }

                m_MasterRouteItems.Add(mrItem);
            }
        }

        /// <summary>
        /// Responds to scroll events from the scrollbar
        /// </summary>
        /// <param name="position">The position in pixels the content of the list should be scrolled to</param>
        private void HandleScroll(int position)
        {
            m_ScrollPosition = position;

            Refresh();
        }

        /// <summary>
        /// Toggle showing the scrollbar
        /// </summary>
        /// <param name="show">True to show the scrollbar, false to hide it</param>
        private void ToggleScrollbar(bool show)
        {
            m_ScrollVisible = show;

            if (show)
                Controls.Add(m_Scroll);
            else
            {
                Controls.Remove(m_Scroll);
                m_ScrollPosition = 0;
            }
        }

        /// <summary>
        /// Return the item at the given point
        /// </summary>
        /// <param name="p">The point used to search for an item in the list</param>
        /// <returns></returns>
        private IListItem ItemAt(Point p)
        {
            foreach (var item in m_MasterRouteItems)
            {
                var found = ItemAt(item, p);
                if (found != null)
                    return found;
            }

            return null;
        }

        /// <summary>
        /// Recursively searches the item and its children to find the item at the given point. Returns
        /// null if no item contains the point.
        /// </summary>
        /// <param name="item">Item to search</param>
        /// <param name="p">The point used to search for an item</param>
        /// <returns></returns>
        private IListItem ItemAt(IListItem item, Point p)
        {
            if (item.Bounds.Contains(p))
                return item;

            if (item.Expanded && item.Children.Count > 0)
                foreach (var child in item.Children)
                {
                    var found = ItemAt(child, p);
                    if (found != null) return found;
                }

            return null;
        }

        #region Windows Events

        protected override void OnFontChanged(EventArgs e)
        {
            Style.Font = Font;
        }

        /// <summary>
        /// Override the paint event to custom draw the list
        /// </summary>
        /// <param name="e"></param>
        protected override void OnPaint(PaintEventArgs e)
        {
            //outline the list
            if (Style.HasBorder)
                e.Graphics.DrawRectangle(
                   Style.BorderPen,
                   ClientRectangle.X,
                   ClientRectangle.Y,
                   ClientRectangle.Width - 1,
                   ClientRectangle.Height - 1
                );

            if (_Model == null) return;

            int count = _Model.MasterRoutes.Count();

            //render the empty message if no items are in the list
            if (count == 0)
            {
                TextRenderer.DrawText(
                    e.Graphics,
                    EmptyMessage,
                    Font,
                    new Rectangle(10, 5, Width - 20, Height - 10),
                    ForeColor,
                    TextFormatFlags.WordBreak | TextFormatFlags.HorizontalCenter | TextFormatFlags.NoPrefix
                );
            }

            //total number of items that would be drawn if the list were infinite height (i.e. even count items that might not be shown due to the scroll position)
            int totalDisplayedItems = m_MasterRouteItems.Sum(item => item.GetDisplayableCount());
            int totalItemHeight = totalDisplayedItems * ITEM_HEIGHT + 6; //padding 3 top, 3 bottom

            //update the scrollbar to the current size of the control and it's contents
            m_Scroll.UpdateBounds(totalItemHeight, ClientRectangle.Height);
            ToggleScrollbar(totalItemHeight > ClientRectangle.Height);

            //determine the items width by accounting for the presence of the scroll bar
            int itemWidth = ClientRectangle.Width - PADDING * 2 - (m_ScrollVisible ? m_Scroll.Width : 0);

            int y = PADDING - m_ScrollPosition;
            //render each item
            foreach (var m in m_MasterRouteItems)
            {
                y = m.Paint(
                    e.Graphics,
                    new Rectangle(PADDING, y, itemWidth, ITEM_HEIGHT),
                    Style
                );
            }
            
        }

        /// <summary>
        /// When the mouse moves over an item on the list we highlight the item.
        /// </summary>
        /// <param name="e"></param>
        protected override void OnMouseMove(MouseEventArgs e)
        {
            var item = ItemAt(e.Location);
            Region invalid = new Region();

            if (item != _HotItem)
            {
                //unhighlight any item that was previously highlighted
                if (_HotItem != null)
                {
                    _HotItem.ExpandHot = false;
                    _HotItem.Highlighted = false;
                    invalid.Union(_HotItem.Bounds);
                }

                _HotItem = item;
                if (item != null) invalid.Union(item.Bounds);
            }

            if (_HotItem != null)
            {
                _HotItem.Highlighted = true;
                bool expandHot = _HotItem.ExpandBox.Contains(e.Location);
                if (expandHot != _HotItem.ExpandHot) invalid.Union(_HotItem.ExpandBox);
                _HotItem.ExpandHot = expandHot;

                _HotItem.HandleMouseMove(e);
            }
            
            Invalidate(invalid);
        }

        /// <summary>
        /// Reset the items such that none of them are highlighted when the mouse leaves
        /// </summary>
        /// <param name="e"></param>
        protected override void OnMouseLeave(EventArgs e)
        {
            foreach (var item in m_MasterRouteItems)
            {
                item.Highlighted = false;
                item.Children.ForEach(c => c.Highlighted = false);
            }

            Refresh();
        }

        /// <summary>
        /// Toggle the selection state of the item under the mouse when the mouse is clicked
        /// </summary>
        /// <param name="e"></param>
        protected override void OnMouseClick(MouseEventArgs e)
        {
            //find the item that was clicked, if any
            IListItem item = ItemAt(e.Location);

            if (item == null) return;
            
            item.HandleMouseClick(e);
        }

        /// <summary>
        /// Adjusts the control in response to being resized
        /// </summary>
        /// <param name="e">General information about the event</param>
        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);

            m_Scroll.Bounds = new Rectangle(ClientRectangle.Width - 16, 1, 15, ClientRectangle.Height - 2);
            m_Scroll.UpdateBounds(m_MasterRouteItems.Count() * ITEM_HEIGHT + 5, Height);
        }

        #endregion Windows Events
        
        /// <summary>
        /// Selects the specified ListItem and adjusts the selection state of it's children and it's parent
        /// as necessary
        /// </summary>
        /// <param name="item">The ListItem to select</param>
        /// <param name="SupressEvent">Value indicating whether a selection event should be triggered</param>
        private void SelectListItem(IListItem item, bool SupressEvent)
        {
            List<IListItem> added = new List<IListItem>();
            List<IListItem> removed = new List<IListItem>();

            //deselect the item if selected (partial or full), select the item if not selected
            ListItemSelectionState newState = item.Selected == ListItemSelectionState.None ? ListItemSelectionState.Selected : ListItemSelectionState.None;
            List<IListItem> l = newState == ListItemSelectionState.None ? removed : added;

            if (item.Selected != newState)
            {
                item.Selected = newState;

                if (item.Children.Count == 0)
                    l.Add(item);
            }

            //update each child to match the selection state of the parent
            if (item.Children.Count > 0)
            {
                foreach (var c in item.Children)
                {
                    if (c.Selected != item.Selected)
                    {
                        c.Selected = item.Selected;
                        l.Add(c);
                    }
                }

                int selCount = item.Children.Count(c => c.Selected != ListItemSelectionState.None);
                item.Selected = selCount ==
                   item.Children.Count
                      ? ListItemSelectionState.Selected
                      : selCount > 0
                         ? ListItemSelectionState.Partial
                         : ListItemSelectionState.None;
            }
            //update the state of the parent to reflect the selection state of the children
            else if (item.Parent != null)
            {
                //if no children are selected then the parent is not selected, if all children are selected then the parent is selected, otherwise the parent is partially selected
                int selectCount = item.Parent.Children.Count(c => c.Selected == ListItemSelectionState.Selected);

                item.Parent.Selected = selectCount == 0 ? ListItemSelectionState.None : (item.Parent.Children.Count == selectCount ? ListItemSelectionState.Selected : ListItemSelectionState.Partial);
            }

            if ((added.Count > 0 || removed.Count > 0) && !SupressEvent)
            {
                //FireSelectionChanged(added.Select(a => a.GetItem<Subroute>()).ToList(), removed.Select(a => a.GetItem<Subroute>()).ToList());
            }
        }
    }

    [TypeConverter(typeof(ExpandableObjectConverter))]
    internal class DetourSelectionListStyle
    {
        public event Action Invalidate;

        #region Brushes and Pens

        //brush used to paint the background of an item that is highlighted
        [Browsable(false)]
        public SolidBrush HighlightBrush { get; private set; } = new SolidBrush(Color.White);
        //brush used to paint the background of an item that is selected
        [Browsable(false)]
        public SolidBrush SelectedBrush { get; private set; } = new SolidBrush(Color.White);

        //brush used to fill in the checkbox when the item is not selected
        [Browsable(false)]
        public SolidBrush CheckUncheckedBrush { get; private set; } = new SolidBrush(Color.Transparent);
        //brush used to fill in the checkbox when the item is selected
        [Browsable(false)]
        public SolidBrush CheckCheckedBrush { get; private set; } = new SolidBrush(Color.Transparent);
        //brush used to fill in the checkbox when the item is partially selected
        [Browsable(false)]
        public SolidBrush CheckCheckedPartialBrush { get; private set; } = new SolidBrush(Color.Transparent);

        //brush used to fill in the expand box when the item is expanded
        [Browsable(false)]
        public SolidBrush ExpandExpandedBrush { get; private set; } = new SolidBrush(Color.Transparent);
        //brush used to fill in the expand box when the item is collapsed
        [Browsable(false)]
        public SolidBrush ExpandCollapsedBrush { get; private set; } = new SolidBrush(Color.Transparent);
        //brush used to fill in the expand box when the mouse is over the item's expand box
        [Browsable(false)]
        public SolidBrush ExpandHotBrush { get; private set; } = new SolidBrush(Color.Transparent);
        //brush used to fill in the background of the item when the item is not highlighted of selected
        [Browsable(false)]
        public SolidBrush BackgroundBrush { get; private set; } = new SolidBrush(Color.Transparent);

        //pen used to draw the border of the list
        [Browsable(false)]
        public Pen BorderPen { get; private set; } = new Pen(Color.White);
        //pen used to draw the check mark of an item
        [Browsable(false)]
        public Pen CheckPen { get; private set; } = new Pen(Color.White);
        //pen used to draw the expand box when the item is collapsed
        [Browsable(false)]
        public Pen ExpandPenCollapsed { get; private set; } = new Pen(Color.White);
        //pen used to draw the expand box when the item is expanded
        [Browsable(false)]
        public Pen ExpandPenExpanded { get; private set; } = new Pen(Color.White);
        //pen used to draw the expand box when the mouse is over the items expand box
        [Browsable(false)]
        public Pen ExpandPenHot { get; private set; } = new Pen(Color.White);

        #endregion Brushes and Pens

        #region Colors

        /// <summary>
        /// The color of the border to be drawn around the control
        /// </summary>
        [Browsable(true)]
        public Color BorderColor { get { return BorderPen.Color; } set { BorderPen.Color = value; Invalidate?.Invoke(); } }

        /// <summary>
        /// Color of the check mark and the check box
        /// </summary>
        [Browsable(true)]
        public Color CheckColor { get { return CheckPen.Color; } set { CheckPen.Color = value; Invalidate?.Invoke(); } }

        /// <summary>
        /// Color of the expand box when the item is collapsed
        /// </summary>
        [Browsable(true)]
        public Color ExpandColorCollapsed { get { return ExpandPenCollapsed.Color; } set { ExpandPenCollapsed.Color = value; Invalidate?.Invoke(); } }

        /// <summary>
        /// Color of the expand box when the item is expanded
        /// </summary>
        [Browsable(true)]
        public Color ExpandColorExpanded { get { return ExpandPenExpanded.Color; } set { ExpandPenExpanded.Color = value; Invalidate?.Invoke(); } }

        /// <summary>
        /// Color of the expand box when the mouse is hovering over it
        /// </summary>
        [Browsable(true)]
        public Color ExpandColorHot { get { return ExpandPenHot.Color; } set { ExpandPenHot.Color = value; Invalidate?.Invoke(); } }

        /// <summary>
        /// Background color of the expand/collapse box when the item is expanded
        /// </summary>
        [Browsable(true)]
        public Color ExpandFillExpanded { get { return ExpandExpandedBrush.Color; } set { ExpandExpandedBrush.Color = value; Invalidate?.Invoke(); } }

        /// <summary>
        /// Background color of the expand/collapse box when the item is collapsed
        /// </summary>
        [Browsable(true)]
        public Color ExpandFillCollapsed { get { return ExpandCollapsedBrush.Color; } set { ExpandCollapsedBrush.Color = value; Invalidate?.Invoke(); } }

        /// <summary>
        /// Background color of the expand/collapse box when the mouse is over it
        /// </summary>
        [Browsable(true)]
        public Color ExpandFillHot { get { return ExpandHotBrush.Color; } set { ExpandHotBrush.Color = value; Invalidate?.Invoke(); } }

        /// <summary>
        /// Background color of the check mark and checkbox when the item is checked
        /// </summary>
        [Browsable(true)]
        public Color CheckFillChecked { get { return CheckCheckedBrush.Color; } set { CheckCheckedBrush.Color = value; Invalidate?.Invoke(); } }

        /// <summary>
        /// Background color of the check mark and checkbox when the item is checked
        /// </summary>
        [Browsable(true)]
        public Color CheckFillCheckedPartial { get { return CheckCheckedPartialBrush.Color; } set { CheckCheckedPartialBrush.Color = value; Invalidate?.Invoke(); } }

        /// <summary>
        /// Background color of the check mark and checkbox when the item is unchecked
        /// </summary>
        [Browsable(true)]
        public Color CheckFillUnchecked { get { return CheckUncheckedBrush.Color; } set { CheckUncheckedBrush.Color = value; Invalidate?.Invoke(); } }

        /// <summary>
        /// Background color of an item in the list when the mouse is hovering over it
        /// </summary>
        [Browsable(true)]
        public Color ItemHighlightColor { get { return HighlightBrush.Color; } set { HighlightBrush.Color = value; Invalidate?.Invoke(); } }

        /// <summary>
        /// Background color of an item in the list when the item is selected
        /// </summary>
        [Browsable(true)]
        public Color ItemSelectedColor { get { return SelectedBrush.Color; } set { SelectedBrush.Color = value; Invalidate?.Invoke(); } }

        #endregion Colors

        private void SetDefaults()
        {
            BorderColor = Color.FromArgb(60, 60, 60);
            CheckColor = Color.FromArgb(55, 70, 140);
            ExpandFillCollapsed = Color.FromArgb(55, 170, 140);
            ExpandFillExpanded = Color.FromArgb(255, 70, 70);
            ExpandFillHot = Color.FromArgb(55, 70, 140);
            ExpandColorHot = Color.Black;
            ExpandColorExpanded = Color.Black;
            ExpandColorCollapsed = Color.Black;
            ItemHighlightColor = Color.FromArgb(51, 153, 255);
        }

        public DetourSelectionListStyle()
        {
            SetDefaults();
        }

        [Browsable(false)]
        public Font Font { get; set; }

        [Browsable(false)]
        public Color TextColor { get; set; }
        
        private bool _HasBorder = true;
        [Browsable(true)]
        public bool HasBorder { get { return _HasBorder; } set { _HasBorder = value; Invalidate?.Invoke(); } }

        private int _ItemIndent;
        [Browsable(true)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        public int ItemIndent { get { return _ItemIndent; } set { _ItemIndent = value; Invalidate?.Invoke(); } }

        public DetourSelectionListItemStyle GetItemStyle(IListItem item)
        {
            return new DetourSelectionListItemStyle()
            {
                ExpandPen = ExpandPenForItem(item),
                CheckPen = CheckPen,
                ExpandBrush = ExpandBrushForItem(item),
                CheckBrush = CheckBrushForItem(item),
                BackgroundBrush = BackgroundBrushForItem(item),
                Font = Font,
                TextColor = TextColor,
                Indent = item.GetLevel() * ItemIndent
            };
        }

        /// <summary>
        /// Returns the pen that should be used to render the expand box for the item base on
        /// the state of the item
        /// </summary>
        /// <param name="item">The item to return a pen for</param>
        /// <returns></returns>
        private Pen ExpandPenForItem(IListItem item)
        {
            if (item.ExpandHot) return ExpandPenHot;
            else if (item.Expanded) return ExpandPenExpanded;
            else return ExpandPenCollapsed;
        }

        /// <summary>
        /// Returns the brush that should be used to fill in the background of the item based on
        /// the state of the item
        /// </summary>
        /// <param name="item">The item to return a brush for</param>
        /// <returns></returns>
        private Brush BackgroundBrushForItem(IListItem item)
        {
            if (item.Highlighted) return HighlightBrush;
            else if (item.Selected == ListItemSelectionState.Selected || item.Selected == ListItemSelectionState.Partial) return SelectedBrush;
            else return BackgroundBrush;
        }

        /// <summary>
        /// Returns a brush used to render the background of the item's checkbox based
        /// on the state of the item
        /// </summary>
        /// <param name="item">The item to return a brush for</param>
        /// <returns></returns>
        private Brush CheckBrushForItem(IListItem item)
        {
            if (item.Selected == ListItemSelectionState.Selected) return CheckCheckedBrush;
            else if (item.Selected == ListItemSelectionState.Partial) return CheckCheckedPartialBrush;
            else return CheckUncheckedBrush;
        }

        /// <summary>
        /// Returns a brush used to render the background of an item's expand box based
        /// on the state of the item
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        private Brush ExpandBrushForItem(IListItem item)
        {
            if (item.ExpandHot) return ExpandHotBrush;
            else if (item.Expanded) return ExpandExpandedBrush;
            else return ExpandCollapsedBrush;
        }
    }

    internal class DetourSelectionListItemStyle
    {
        public Pen ExpandPen { get; set; }
        public Pen CheckPen { get; set; }
        public Brush ExpandBrush { get; set; }
        public Brush CheckBrush { get; set; }
        public Brush BackgroundBrush { get; set; }
        public Color TextColor { get; set; }
        public Font Font { get; set; }
        public int Indent { get; set; }
    }

    /// <summary>
    /// Enumeration of possible selection states for list items
    /// </summary>
    enum ListItemSelectionState { Selected, None, Partial }

    /// <summary>
    /// Interface shared by all items in the list
    /// </summary>
    internal interface IListItem
    {
        event Action<IListItem> Invalidate;

        /// <summary>
        /// The item's parent in the list, null if the item is top level
        /// </summary>
        IListItem Parent { get; set; }

        /// <summary>
        /// The item's children, empty if the item is a leaf
        /// </summary>
        List<IListItem> Children { get; set; }

        /// <summary>
        /// The text the item displays
        /// </summary>
        string Text { get; set; }

        /// <summary>
        /// The bounds of the item
        /// </summary>
        Rectangle Bounds { get; set; }

        /// <summary>
        /// The bounds of the item's checkbox
        /// </summary>
        Rectangle CheckBox { get; set; }

        /// <summary>
        /// The bounds of the item's expand box
        /// </summary>
        Rectangle ExpandBox { get; set; }

        /// <summary>
        /// Value indicating if the item is highlighted
        /// </summary>
        bool Highlighted { get; set; }

        /// <summary>
        /// Value indicating if the item is checked
        /// </summary>
        bool Checked { get; set; }

        /// <summary>
        /// Value indicating if the item is expanded
        /// </summary>
        bool Expanded { get; set; }

        /// <summary>
        /// Value indicating if the mouse is over the item's expand box
        /// </summary>
        bool ExpandHot { get; set; }

        /// <summary>
        /// Value indicating the selection state of the item. An item is partially selected if some, but not all, of it's
        /// children are selected.
        /// </summary>
        ListItemSelectionState Selected { get; set; }

        /// <summary>
        /// Retrieves the item represented by list item if it matches the type specified. Returns
        /// null if the types do not match.
        /// </summary>
        /// <typeparam name="T">The type of object to return</typeparam>
        /// <returns></returns>
        T GetItem<T>() where T : class;

        int GetDisplayableCount();

        int GetChildCount();

        int GetSelectedChildCount();

        int GetLevel();

        void HandleMouseMove(MouseEventArgs e);

        void HandleMouseClick(MouseEventArgs e);

        /// <summary>
        /// Renders the list item and its children using the specified values
        /// </summary>
        /// <param name="g">The graphics object used to render the item</param>
        /// <param name="bounds">The bounds of the list item</param>
        /// <param name="font">The font used to render the text of the list item</param>
        /// <param name="background">The brush used to render the background of the item</param>
        /// <param name="textColor">The color used to render the text of the item</param>
        /// <param name="checkPen">The pen used to render the checkbox of the item</param>
        /// <param name="checkBrush">The brush used to render the background of the checkbox of the item</param>
        /// <param name="expandPen">The pen used to render the expand box of the item</param>
        /// <param name="expandBrush">The brush used to render the background of the expand box of the item</param>
        int Paint(Graphics g, Rectangle bounds, DetourSelectionListStyle style);
    }

    internal class ListItem<T> : IListItem where T : class
    {
        public event Action<IListItem> Invalidate;

        protected void FireInvalidate()
        {
            Invalidate?.Invoke(this);
        }

        private const int CHECK_BOX_SIZE = 12;

        /// <summary>
        /// The item being represented
        /// </summary>
        public T Item { get; set; }

        /// <summary>
        /// The text displayed by the item
        /// </summary>
        public virtual string Text { get; set; }

        /// <summary>
        /// The bounds of the item
        /// </summary>
        public Rectangle Bounds { get; set; } = new Rectangle();

        /// <summary>
        /// The bounds of the item's checkbox
        /// </summary>
        public Rectangle CheckBox { get; set; }

        /// <summary>
        /// The bounds of the item's expand box
        /// </summary>
        public Rectangle ExpandBox { get; set; }

        /// <summary>
        /// The selection state of the item
        /// </summary>
        public virtual ListItemSelectionState Selected { get; set; } = ListItemSelectionState.None;

        /// <summary>
        /// Value indicating if the item is highlighted
        /// </summary>
        public bool Highlighted { get; set; }

        /// <summary>
        /// Value indicating if the item is expanded
        /// </summary>
        public bool Expanded { get; set; }

        /// <summary>
        /// Value indicating if the item is checked
        /// </summary>
        public bool Checked { get; set; }

        /// <summary>
        /// Value indicating if the mouse is over the item's expand box
        /// </summary>
        public bool ExpandHot { get; set; }

        /// <summary>
        /// The item's parent ListItem, null if the item is top level
        /// </summary>
        public IListItem Parent { get; set; }

        /// <summary>
        /// List of the item's children, empty if the item is a leaf
        /// </summary>
        public List<IListItem> Children { get; set; } = new List<IListItem>();
        
        /// <summary>
        /// Default constructor
        /// </summary>
        public ListItem() { }

        /// <summary>
        /// Constructs a new list item from the given item and text
        /// </summary>
        /// <param name="item">The item the list item represents</param>
        /// <param name="text">The text the item displays</param>
        public ListItem(T item, string text)
           : this()
        {
            Item = item;
            Text = item.ToString();
        }
        
        public int GetDisplayableCount()
        {
            return 1 + ((Children.Count > 0 && Expanded) ? Children.Sum(c => c.GetDisplayableCount()) : 0);
        }

        public int GetChildCount()
        {
            return Children.Count + (Children.Count > 0 ? Children.Sum(c => c.GetChildCount()) : 0);
        }

        public int GetSelectedChildCount()
        {
            return Children.Count(c => c.Checked) + (Children.Count > 0 ? Children.Sum(c => c.GetSelectedChildCount()) : 0);
        }

        public int GetLevel()
        {
            var parent = Parent;
            int level = 0;
            while (parent != null)
            {
                level += 1;
                parent = parent.Parent;
            }

            return level;
        }

        public virtual void HandleMouseMove(MouseEventArgs e) { }

        public virtual void HandleMouseClick(MouseEventArgs e)
        {
            //if the expand box was clicked, toggle the expansion state of the item
            if (ExpandBox.Contains(e.Location) && Children.Count > 0)
            {
                Expanded = !Expanded;
                FireInvalidate();
            }
            else
            {
                Selected = Item is DetourMasterRoute ? ListItemSelectionState.Partial : ListItemSelectionState.Selected;
            }
        }
        
        public void Unselect()
        {

        }
        
        /// <summary>
        /// Return the item this lists item represents or null if the requested type does not match the type
        /// of the item represented by this list item
        /// </summary>
        /// <typeparam name="V">The type expected by the caller</typeparam>
        /// <returns></returns>
        public V GetItem<V>() where V : class { if (typeof(T) == typeof(V)) return Item as V; else return default(V); }

        /// <summary>
        /// Renders the list item using the specified values
        /// </summary>
        /// <param name="g">The graphics object used to render the item</param>
        /// <param name="bounds">The bounds of the list item</param>
        /// <param name="font">The font used to render the text of the list item</param>
        /// <param name="background">The brush used to render the background of the item</param>
        /// <param name="textColor">The color used to render the text of the item</param>
        /// <param name="checkPen">The pen used to render the checkbox of the item</param>
        /// <param name="checkBrush">The brush used to render the background of the checkbox of the item</param>
        /// <param name="expandPen">The pen used to render the expand box of the item</param>
        /// <param name="expandBrush">The brush used to render the background of the expand box of the item</param>
        public virtual int Paint(Graphics g, Rectangle bounds, DetourSelectionListStyle style)
        {
            var myStyle = style.GetItemStyle(this);

            Bounds = bounds;
            ExpandBox = new Rectangle(bounds.X + 5 + myStyle.Indent, bounds.Y + (bounds.Height - 10) / 2, 10, 10);
            CheckBox = CheckBox = new Rectangle(
                ExpandBox.Right + 5,
                bounds.Y + (bounds.Height - CHECK_BOX_SIZE) / 2,
                CHECK_BOX_SIZE,
                CHECK_BOX_SIZE);
            
            Size textSize = TextRenderer.MeasureText(g, Text, myStyle.Font);
            Rectangle textRect = new Rectangle(
                  CheckBox.Right + 2,
                  bounds.Y + (bounds.Height - textSize.Height) / 2 + 1,
                  bounds.Width - CheckBox.Width - 7,
                  bounds.Height
               );

            //paint list background
            g.FillRectangle(myStyle.BackgroundBrush, bounds);

            //render expand collapse box based on child count
            if (Children.Count > 0)
                Painter.RenderExpandBox(g, ExpandBox, myStyle.ExpandPen, myStyle.ExpandBrush, Expanded);

            Painter.RenderCheckBox(g, CheckBox, myStyle.CheckPen, myStyle.CheckBrush, Selected);
            Painter.RenderText(g, myStyle.Font, Text, textRect, myStyle.TextColor);

            int y = bounds.Bottom;
            if (Expanded)
            {
                Rectangle childBounds = new Rectangle(bounds.X, y, bounds.Width, bounds.Height);
                foreach (var child in Children)
                {
                    y = child.Paint(g, childBounds, style);
                    childBounds = new Rectangle(bounds.X, y, bounds.Width, bounds.Height);
                }
            }
            return y;
        }
    }

    internal class Painter
    {
        internal static void RenderText(Graphics g, Font font, string text, Rectangle textRect, Color textColor)
        {
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;
            TextRenderer.DrawText(
               g,
               text,
               font,
               textRect,
               textColor,
               TextFormatFlags.Left | TextFormatFlags.EndEllipsis | TextFormatFlags.NoPrefix
            );
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.SystemDefault;

        }
        internal static void RenderCheckBox(Graphics g, Rectangle bounds, Pen checkPen, Brush checkBrush, ListItemSelectionState state)
        {
            checkPen.Width = 1f;
            g.FillRectangle(checkBrush, bounds);
            g.DrawRectangle(checkPen, bounds);

            if (state == ListItemSelectionState.Selected)
            {
                checkPen.Width = 2f;
                g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                g.DrawLine(checkPen, bounds.X + 2, bounds.Y + bounds.Height / 2, bounds.X + bounds.Width / 3, bounds.Y + bounds.Height - 2);
                g.DrawLine(checkPen, bounds.X + bounds.Width / 3, bounds.Y + bounds.Height - 2, bounds.X + bounds.Width - 2, bounds.Y + 2);
                g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.Default;
            }
            else if (state == ListItemSelectionState.Partial)
            {
                Rectangle fill = new Rectangle(bounds.X + 2, bounds.Y + 2, bounds.Width - 3, bounds.Height - 3);
                g.FillRectangle(new SolidBrush(checkPen.Color), fill);
            }
        }
        internal static void RenderCancelButton(Graphics g, Rectangle bounds, Pen pen, Brush brush)
        {
            int Space = 3;

            g.FillRectangle(brush, bounds);
            g.DrawRectangle(Pens.Black, bounds);

            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            g.DrawLine(pen, bounds.X + Space, bounds.Y + Space, bounds.X + bounds.Width - Space, bounds.Y + bounds.Height - Space);
            g.DrawLine(pen, bounds.X + bounds.Width - Space, bounds.Y + Space, bounds.X + Space, bounds.Y + bounds.Height - Space);
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.Default;
        }
        internal static void RenderExpandBox(Graphics g, Rectangle bounds, Pen expandPen, Brush expandBrush, bool expanded)
        {
            g.FillRectangle(expandBrush, bounds);
            g.DrawRectangle(expandPen, bounds);

            //render + or - depending on expansion state
            g.DrawLine(expandPen, bounds.Left + 2, (bounds.Bottom + bounds.Top) / 2, bounds.Right - 2, (bounds.Bottom + bounds.Top) / 2);
            if (!expanded)
            {
                g.DrawLine(expandPen, (bounds.Left + bounds.Right) / 2, bounds.Top + 2, (bounds.Left + bounds.Right) / 2, bounds.Bottom - 2);
            }
        }
    }

    internal class MasterRouteListItem : ListItem<DetourMasterRoute>
    {
        public MasterRouteListItem(DetourMasterRoute mr) : base(mr, mr.Name) { }

        public override string Text
        {
            get { return Item.Name; }
            set { }
        }

        //public override ListItemSelectionState Selected
        //{
        //    get
        //    {
        //        int childCount = GetChildCount();
        //        int selectedChildCount = GetSelectedChildCount();

        //        return childCount == selectedChildCount
        //            ? ListItemSelectionState.Selected
        //            : selectedChildCount > 0
        //                ? ListItemSelectionState.Partial
        //                : ListItemSelectionState.None;
        //    }
        //    set { }
        //}
    }

    internal class SubrouteListItem : ListItem<DetourSubroute>
    {
        public SubrouteListItem(DetourSubroute sr) : base(sr, sr.Name) { }

        public override string Text
        {
            get { return Item.Name; }
            set { }
        }
    }

    internal class StopListItem : ListItem<DetourStop>
    {
        private Rectangle _CancelBox = new Rectangle();

        private static readonly Color CanceledColor = Color.FromArgb(155, 52, 48);
        private static readonly Color UncanceledColor = Color.FromArgb(20, 122, 24);

        private bool _CancelButtonHot = false;

        public StopListItem(DetourStop stop) : base(stop, stop.Name)
        {
            stop.PropertyChanged += (o, e) =>
            {
                if (e.PropertyName == "Canceled")
                {
                    FireInvalidate();
                }
            };
        }

        public override int Paint(Graphics g, Rectangle bounds, DetourSelectionListStyle style)
        {
            Bounds = bounds;
            var myStyle = style.GetItemStyle(this);

            //if (Highlighted)
            //    g.FillRectangle(myStyle.BackgroundBrush, bounds);

            string cancelText = Item.Canceled ? "Uncancel" : "Cancel";
            Size cancelTextSize = TextRenderer.MeasureText(cancelText, myStyle.Font);

            Size textSize = TextRenderer.MeasureText(Text, style.Font);
            Rectangle textBox = new Rectangle(bounds.X + 3 + myStyle.Indent, bounds.Y + (bounds.Height - textSize.Height) / 2, textSize.Width, textSize.Height);

            _CancelBox = new Rectangle(
                textBox.Right + 3,
                bounds.Y + 2,
                cancelTextSize.Width + 6,
                bounds.Height - 4
            );
            Rectangle cancelTextBox = new Rectangle(
                _CancelBox.X + 3,
                _CancelBox.Y + (_CancelBox.Height - cancelTextSize.Height) / 2,
                cancelTextSize.Width,
                cancelTextSize.Height
            );

            using (Brush b = Item.Canceled ? new SolidBrush(UncanceledColor) : new SolidBrush(CanceledColor))
                g.FillRectangle(_CancelButtonHot ? myStyle.BackgroundBrush : b, _CancelBox);

            g.DrawRectangle(Pens.Black, _CancelBox);

            Painter.RenderText(g, myStyle.Font, cancelText, cancelTextBox, Color.White);

            Color c = Item.Canceled ? CanceledColor : myStyle.TextColor;
            if (Item.Canceled)
            {
                using (Font f = new Font(myStyle.Font, FontStyle.Strikeout))
                    Painter.RenderText(g, f, Text, textBox, c);
            }
            else
                Painter.RenderText(g, myStyle.Font, Text, textBox, c);

            return bounds.Bottom;
        }

        public override void HandleMouseClick(MouseEventArgs e)
        {
            base.HandleMouseClick(e);

            if (_CancelBox.Contains(e.Location))
            {
                Item.Canceled = !Item.Canceled;
            }
        }

        public override void HandleMouseMove(MouseEventArgs e)
        {
            bool hit = _CancelBox.Contains(e.Location);

            if (hit != _CancelButtonHot)
            {
                _CancelButtonHot = hit;
                FireInvalidate();
            }
        }

        public override string Text
        {
            get { return Item.Name; }
            set { }
        }
    }
}