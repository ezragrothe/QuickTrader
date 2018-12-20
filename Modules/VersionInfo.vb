Module VersionInfo
    Public ReadOnly Property Version As String
        Get
            Return "3.01.16"
        End Get
    End Property
    Public ReadOnly Property CurrentVersionIndex As Integer
        Get
            Return 25
        End Get
    End Property
    Public ReadOnly Property VersionChanges As List(Of String)
        Get
            Dim lst As New List(Of String)
            lst.Add("Fixed bugs with backed up channel lines on the AutoTrend")
            lst.Add("Changed the last channel width to 1.5 pixels")
            lst.Add("Changed the method of switching charts to the scroll wheel, instead of double clicking")
            lst.Add("Changed the AutoTrendPad to update instantly when settings are changed in the Format Chart box")
            lst.Add("Removed the last two rows of the AutoTrendPad")
            lst.Add("Changed the AutoTrendPad to round the numbers of the RV buttons to the nearest digit")
            lst.Add("Made the AutoTrendPad much more compact")
            Return lst
        End Get
    End Property
    Public ReadOnly Property Version30113Changes As List(Of String)
        Get
            Dim lst As New List(Of String)
            lst.Add("Changed the selection circles of chart lines to be inverse color of the background")
            lst.Add("Added 3 more order quantity presets")
            lst.Add("Changed channel lines to not change color when their BC length was hit without being cut")
            lst.Add("Fixed bug with realtime bars being shown while the HideBars option is checked")
            lst.Add("Increased performance of analysis techniques slightly")
            Return lst
        End Get
    End Property
    Public ReadOnly Property Version21225Changes As List(Of String)
        Get
            Dim lst As New List(Of String)
            lst.Add("Changed AutoTrend merged mode to color swings and bars according to trend mode")
            lst.Add("Potential BC length texts are now shown on potential channels on the AutoTrend")
            lst.Add("Gapped channels now always show the parallel when they are hit")
            Return lst
        End Get
    End Property
    Public ReadOnly Property Version21222Changes As List(Of String)
        Get
            Dim lst As New List(Of String)
            lst.Add("Added option on the AutoTrend to display BC length text history")
            lst.Add("Fixed how AutoTrend price slave charts operate so that the prices line up")
            lst.Add("Added option on the AutoTrend to remove gapped channel line parallels")
            Return lst
        End Get
    End Property
    Public ReadOnly Property Version21201Changes As List(Of String)
        Get
            Dim lst As New List(Of String)
            lst.Add("Updated cache system to enable using TradeStation data")
            lst.Add("Fixed bug with channel lines not showing in some cases")
            lst.Add("Changed colors of RVPad buttons")
            lst.Add("Changed RV2 and RV3 RVPad buttons to always be highlighted red")
            lst.Add("Changed overlap markers to match gap marker thicknesses")
            lst.Add("Added option for the visibility of gap markers")
            lst.Add("Added overlap markers to swing mode")
            Return lst
        End Get
    End Property
    Public ReadOnly Property Version21113Changes As List(Of String)
        Get
            Dim lst As New List(Of String)
            lst.Add("PriceRVSlaveCharts now will have the same RV as the master")
            lst.Add("Fixed bug with gap markers on the last price")
            lst.Add("Fixed bug with gap markers not displaying properly with swing mode")
            lst.Add("Changed trend mode swing lines to be colored as inside/outside")
            Return lst
        End Get
    End Property
    Public ReadOnly Property Version21111Changes As List(Of String)
        Get
            Dim lst As New List(Of String)
            lst.Add("Changed thickness of swing mode swing lines to 2")
            lst.Add("Increased efficiency of AutoTrend slave charts")
            lst.Add("Added option to hide projection lines")
            Return lst
        End Get
    End Property
    Public ReadOnly Property Version21110Changes As List(Of String)
        Get
            Dim lst As New List(Of String)
            lst.Add("Changed PriceAutoTrendSlaveCharts to follow the price of the master AutoTrend's chart price")
            lst.Add("Removed all hotkey functionality in the AutoTrend")
            lst.Add("Added options on the AutoTrend for inside and outside swings while in swing mode")
            Return lst
        End Get
    End Property
    Public ReadOnly Property Version21109Changes As List(Of String)
        Get
            Dim lst As New List(Of String)
            lst.Add("Added option to control when AutoTrend starts execution")
            lst.Add("Changed AutoTrend to control slave charts from the beginning regardless of execution start point")
            lst.Add("Changed slave AutoTrend charts to hide the chart bars")
            lst.Add("Added option for swing label's text on the AutoTrend")
            lst.Add("Fixed bug with AutoTrendBarCharts swing lengths to be one less than actual bar count")
            Return lst
        End Get
    End Property
    Public ReadOnly Property Version21107Changes As List(Of String)
        Get
            Dim lst As New List(Of String)
            lst.Add("Added AutoTrend price slave chart capability")
            lst.Add("Fixed the AutoTrend slave charts to apply times correctly to bars (TimeBox will now work)")
            lst.Add("Changed the AutoTrend slave charts to pivot the swings on only one bar")
            lst.Add("Fixed bug with bar count being displayed incorrectly during real time price changes")
            lst.Add("Fixed bug with AutoTrend slave charts not displaying analysis techniques correctly")
            Return lst
        End Get
    End Property
    Public ReadOnly Property Version21105Changes As List(Of String)
        Get
            Dim lst As New List(Of String)
            lst.Add("Added a very beta AutoTrend bar slave chart feature")
            lst.Add("Changed the look of the Order Bar slightly")
            lst.Add("Shortened the length of order control reference lines by 30%")
            Return lst
        End Get
    End Property
    Public ReadOnly Property Version21101Changes As List(Of String)
        Get
            Dim lst As New List(Of String)
            lst.Add("Moved order controls to the left side the last bar")
            lst.Add("Changed AutoTrend to start 100 bars before first bar visible")
            lst.Add("Centered swing length texts on swing points")
            lst.Add("Fixed bug with target texts on the AutoTrend")
            Return lst
        End Get
    End Property
    Public ReadOnly Property Version20928Changes As List(Of String)
        Get
            Dim lst As New List(Of String)
            lst.Add("Changed high of day and low of day texts on the TimeBox to be bigger and bolder")
            lst.Add("Changed AutoTrendPad RV buttons to be highlighted by right-click")
            lst.Add("Added a swing mode on the AutoTrend")
            Return lst
        End Get
    End Property
    Public ReadOnly Property Version20916Changes As List(Of String)
        Get
            Dim lst As New List(Of String)
            lst.Add("Added an AutoTrendPad for charts")
            Return lst
        End Get
    End Property
    Public ReadOnly Property Version20913Changes As List(Of String)
        Get
            Dim lst As New List(Of String)
            lst.Add("Improved coloring of non-merged channels")
            lst.Add("Added an extension projection line on the AutoTrend")
            lst.Add("Added input for projection lines on the AutoTrend")
            lst.Add("Moved the high of day indicator text on the TimeBox to the top of each day box")
            Return lst
        End Get
    End Property
    Public ReadOnly Property Version20831Changes As List(Of String)
        Get
            Dim lst As New List(Of String)
            lst.Add("Improved the single channel coloring mode")
            lst.Add("Changed snap RV to 2x range to be a toggle")
            Return lst
        End Get
    End Property
    Public ReadOnly Property Version20829Changes As List(Of String)
        Get
            Dim lst As New List(Of String)
            lst.Add("Increased size of the popup info box that displays when a chart object is clicked")
            lst.Add("Added labels indicating the low and high of days on the TimeBox")
            Return lst
        End Get
    End Property
    Public ReadOnly Property Version20827Changes As List(Of String)
        Get
            Dim lst As New List(Of String)
            lst.Add("Removed the Next Day input for the TimeBox and made it automatically selected")
            lst.Add("Changed the height for the projected box on the TimeBox to be time/percentage based")
            lst.Add("Fixed occasional bug with swing channels on the AutoTrend")
            lst.Add("Added hotkey to AutoTrend to toggle coloring merged channels")
            Return lst
        End Get
    End Property
    Public ReadOnly Property Version20821Changes As List(Of String)
        Get
            Dim lst As New List(Of String)
            lst.Add("Changed the selected color for filled orders to light colored colors, rather than white")
            lst.Add("Added timeout feature of 15 seconds for analysis techniques")
            Return lst
        End Get
    End Property
    Public ReadOnly Property Version20816Changes As List(Of String)
        Get
            Dim lst As New List(Of String)
            lst.Add("Fixed rare rounding bug for confirmed channels on AutoTrend")
            lst.Add("Added projection range text to TimeBox")
            lst.Add("Increased performance of TimeBox by about 3 times")
            lst.Add("Fixed bug where bars occasionally jump past the right margin")
            lst.Add("Fixed issue where the log window does not remember last position")
            lst.Add("Added option on application tab of the format window to hide the log window after 3 seconds of showing")
            lst.Add("Adjusted the colors for buy stop and sell stop orders to look better with white chart backgrounds")
            Return lst
        End Get
    End Property
    Public ReadOnly Property Version20815Changes As List(Of String)
        Get
            Dim lst As New List(Of String)
            lst.Add("Changed arrows and labels to be selected when they are created")
            lst.Add("Changed the text object default anchor point to the bottom right corner")
            lst.Add("Added input on TimeBox for displaying the range on each day box")
            lst.Add("Added feature to scale to data when bars are loaded on a chart")
            Return lst
        End Get
    End Property
    Public ReadOnly Property Version20809Changes As List(Of String)
        Get
            Dim lst As New List(Of String)
            lst.Add("Improved scaling method")
            lst.Add("Changed popup info box for controls to display in the top right corner of the chart")
            Return lst
        End Get
    End Property
    Public ReadOnly Property Version20808Changes As List(Of String)
        Get
            Dim lst As New List(Of String)
            lst.Add("Added feature to save arrows and orders on a chart when it is refreshed")
            lst.Add("Changed the AutoTrend projection RV line to be solid")
            lst.Add("Added input on TimeBox for number of days to use as projection average")
            lst.Add("Added button to cancel closing a workspace when it is closed")
            Return lst
        End Get
    End Property
    Public ReadOnly Property Version20731Changes As List(Of String)

        Get
            Dim lst As New List(Of String)
            lst.Add("Changed gap markers to on the AutoTrend to be one pixel thicker")
            lst.Add("Changed the TimeBox Toggle Direction hotkey to toggle the direction of all instances of TimeBox in the current workspace")
            lst.Add("Removed the confirmation box that displays when you want to apply analysis technique settings to all instances")
            lst.Add("Possibly improved the application force update code to prevent thread collisions")
            lst.Add("Added 'View Update History' link on the about window")
            Return lst
        End Get
    End Property
    Public ReadOnly Property Version20730Changes As List(Of String)
        Get
            Dim lst As New List(Of String)
            lst.Add("Added inputs for the RVText color on the AutoTrend and the line colors for the PriceLines analysis technique")
            lst.Add("Fixed error lines with analysis techniques that show up when a chart is refreshed while market data is being received")
            lst.Add("Added feature to stop execution of an analysis technique if it is executing for more than 15 seconds")
            Return lst
        End Get
    End Property
    Public ReadOnly Property Version20727Changes As List(Of String)
        Get
            Dim lst As New List(Of String)
            lst.Add("Fixed several major bugs with range bar calulation")
            lst.Add("Fixed a bug with the AutoTrend trend calculation")
            lst.Add("Changed auto-reload feature for charts to activate when bar count exceeds desired bar count by 100")
            Return lst
        End Get
    End Property
    Public ReadOnly Property Version20726Changes As List(Of String)
        Get
            Dim lst As New List(Of String)
            lst.Add("Improved program performance by reducing number of times memory was cleaned")
            lst.Add("Added plot drawing object for analysis techniques to use")
            lst.Add("Added MovingAverage analysis technique as a base point for more complex averaging techniques")
            Return lst
        End Get
    End Property
    Public ReadOnly Property Version20725Changes As List(Of String)
        Get
            Dim lst As New List(Of String)
            lst.Add("Added option for desired number of bars on a chart")
            lst.Add("Added feature to automatically refresh chart if bar count exceeds 50 more than the desired number of bars on that chart")
            Return lst
        End Get
    End Property
    Public ReadOnly Property Version20723Changes As List(Of String)
        Get
            Dim lst As New List(Of String)
            lst.Add("Added  projection end line for the time box")
            lst.Add("Added option on time box to disable Sundays")
            Return lst
        End Get
    End Property
    <VersionIndex(25)> Public ReadOnly Property Version20716Changes As List(Of String)
        Get
            Dim lst As New List(Of String)
            lst.Add("Changed AutoTrend to hide last swing line and and last confirmed channel CD line")
            lst.Add("Changed TimeBox to show projection box as average of last 5 days, rather than 3")
            lst.Add("Fixed occasional bug with range bar calculation")
            lst.Add("Improved TimeBox handling of Sun/Mon on full day sessions")
            Return lst
        End Get
    End Property
    <VersionIndex(24)> Public ReadOnly Property Version20712Changes As List(Of String)
        Get
            Dim lst As New List(Of String)
            lst.Add("Added projection range lines to the TimeBox")
            lst.Add("Increased efficiency for toggling the anchor point on the TimeBox")
            lst.Add("Renamed the 'Snap Scaling To Fit Bars' command to 'Snap Scaling To All Data'")
            lst.Add("Increased functionality for the 'Snap Scaling To All Data' command")
            Return lst
        End Get
    End Property
    <VersionIndex(23)> Public ReadOnly Property Version20703Changes As List(Of String)
        Get
            Dim lst As New List(Of String)
            lst.Add("Added feature to hide the log box after 3 seconds")
            lst.Add("Changed AutoTrend to no longer show confirmed, queued channel AB and CD swings")
            Return lst
        End Get
    End Property
    <VersionIndex(22)> Public ReadOnly Property Version20702Changes As List(Of String)
        Get
            Dim lst As New List(Of String)
            lst.Add("Fixed bug with analysis techniques refreshing with a tick movement while loading history")
            lst.Add("Added hotkey to toggle the anchor point for the TimeBox")
            lst.Add("Added highlighted swings for the current trend on the AutoTrend")
            Return lst
        End Get
    End Property
    <VersionIndex(21)> Public ReadOnly Property Version20628Changes As List(Of String)
        Get
            Dim lst As New List(Of String)
            lst.Add("Fixed bug with time box lines not being shortened on completion")
            lst.Add("Added feature to set the RV for the AutoTrend to 2.5 * the range when the range is incremented")
            Return lst
        End Get
    End Property
    <VersionIndex(20)> Public ReadOnly Property Version20627Changes As List(Of String)
        Get
            Dim lst As New List(Of String)
            lst.Add("Added projected end line for the TimeBox analysis technique")
            lst.Add("If the start and end lines for consecutive time boxes are one bar apart, they are now drawn on the same bar")
            lst.Add("The order bar can no longer be set to transparent")
            lst.Add("Added hotkey to increase and decrease the range for a symbol")
            Return lst
        End Get
    End Property
    <VersionIndex(19)> Public ReadOnly Property Version20621Changes As List(Of String)
        Get
            Dim lst As New List(Of String)
            lst.Add("Changed the color of buy stop orders to a brighter blue")
            lst.Add("Added feature to sync the default order quantity for all charts in a workspace of the same symbol")
            lst.Add("Increased efficiency of loading cache data by about 4 times")
            Return lst
        End Get
    End Property
    <VersionIndex(18)> Public ReadOnly Property Version20620Changes As List(Of String)
        Get
            Dim lst As New List(Of String)
            lst.Add("Removed the old AutoTrend analysis technique")
            lst.Add("Added text showing how many days back of cache data are available on the format chart window")
            lst.Add("Possibly fixed bug with confirmed cut channels on the AutoTrend analysis techinique")
            lst.Add("Fixed bug with not being able to load all the data in the cache file if IB returns more than was called for")
            Return lst
        End Get
    End Property
    <VersionIndex(17)> Public ReadOnly Property Version20615Changes As List(Of String)
        Get
            Dim lst As New List(Of String)
            lst.Add("Added 'Highlight Last Cut Channel' input to AutoTrendV2")
            lst.Add("Changed the color of buy stop orders to a brighter blue")
            lst.Add("Added caching feature")
            Return lst
        End Get
    End Property
    <VersionIndex(16)> Public ReadOnly Property Version20513Changes As List(Of String)
        Get
            Dim lst As New List(Of String)
            lst.Add("Added input for color on the TimeBox analysis technique")
            lst.Add("Added 'Remove Parallel' command")
            lst.Add("Added highlighting of last cut channel feature to AutoTrendV2")
            lst.Add("The default setting values for the first time QuickTrader is opened are now more intuitive")
            Return lst
        End Get
    End Property
    <VersionIndex(15)> Public ReadOnly Property Version20509Changes As List(Of String)
        Get
            Dim lst As New List(Of String)
            lst.Add("'Price Text Scaling' and 'Time Text Scaling' changed to 'Price Scaling' and 'Time Scaling'")
            lst.Add("All channels are removed on the AutoTrend analysis technique when their starting point is off the left of the screen")
            lst.Add("All workspaces now don't ask to save when 'Close and Save All Desktops' is clicked")
            Return lst
        End Get
    End Property
    <VersionIndex(14)> Public ReadOnly Property Version20507Changes As List(Of String)
        Get
            Dim lst As New List(Of String)
            lst.Add("Fixed bugs on AutoTrendV2")
            lst.Add("Added SwingRV feature to AutoTrendV2")
            Return lst
        End Get
    End Property
    <VersionIndex(13)> Public ReadOnly Property Version20506Changes As List(Of String)
        Get
            Dim lst As New List(Of String)
            lst.Add("Historical data for all symbols is now only shown if between 8:30 am and 3:00 pm")
            lst.Add("Trend lines and their parallels are now much more efficient")
            lst.Add("Updated AutoTrendV1 to handle the new trend line handling")
            lst.Add("Added AutoTrendV2 analysis technique")
            lst.Add("When workspaces or charts are removed, charts now remove all event handling, to prevent ID conflicts")
            lst.Add("The AutoTrend SwingRV input now is automatically always set to 1.5 x the range value on charts")
            lst.Add("Made order box to be opaque")
            lst.Add("Fixed bug with formatting the TimeBox analysis technique when realtime data is being recieved")
            Return lst
        End Get
    End Property
    <VersionIndex(12)> Public ReadOnly Property Version20421Changes As List(Of String)
        Get
            Dim lst As New List(Of String)
            lst.Add("Added error support for outdated workspaces")
            lst.Add("Brought back 'Swing Channel RV' input for the 'AutoTrend' analysis technique")
            lst.Add("Fixed bug with projection line on the 'AutoTrend' analysis technique on slave charts")
            lst.Add("Channel lines on the 'AutoTrend' analysis technique now are hidden if their start point is off the left side of the chart")
            Return lst
        End Get
    End Property
    <VersionIndex(11)> Public ReadOnly Property Version20419Changes As List(Of String)
        Get
            Dim lst As New List(Of String)
            lst.Add("Fixed numerous bugs with master/slave chart systems")
            lst.Add("Fixed bugs with saving/reloading charts")
            lst.Add("Changed status text to stretch across the top of the chart")
            Return lst
        End Get
    End Property
    <VersionIndex(10)> Public ReadOnly Property Version20418Changes As List(Of String)
        Get
            Dim lst As New List(Of String)
            lst.Add("Added input to turn off swing channel lines on the 'AutoTrend' analysis techqniue")
            lst.Add("Changed pan up and down hotkeys to pan 1.5% of the chart height")
            Return lst
        End Get
    End Property
    <VersionIndex(9)> Public ReadOnly Property Version20417Changes As List(Of String)
        Get
            Dim lst As New List(Of String)
            lst.Add("Cleaned up master slave code, fixed bugs")
            lst.Add("Added 'Status Text Background' setting for charts")
            lst.Add("Fixed bug with exactly vertical trend lines when they are extended")
            Return lst
        End Get
    End Property
    <VersionIndex(8)> Public ReadOnly Property Version20415Changes As List(Of String)
        Get
            Dim lst As New List(Of String)
            lst.Add("Added feature to load data onto slave chart without affecting master chart")
            lst.Add("A slave chart's status text no longer shows range bar information")
            lst.Add("The bar size for slave charts is now always the same as their master chart's bar size")
            lst.Add("More than one slave chart now works correctly with one master")
            lst.Add("Added spacer bars feature on slave charts when they lag the master chart")
            lst.Add("Fixed bugs with AutoTrend channel lines")
            lst.Add("Possibly fixed bug with real time data on slave charts")
            Return lst
        End Get
    End Property
    <VersionIndex(7)> Public ReadOnly Property Version20413Changes As List(Of String)
        Get
            Dim lst As New List(Of String)
            lst.Add("Charts can now cancel loading historical data")
            lst.Add("Made minor change to 'AutoTrend' analysis techinque channel line handling")
            lst.Add("Made safeguard to multiple bar drawing because of conflicting ticker IDs")
            lst.Add("Improved and fixed issues with Master/Slave chart feature --")
            lst.Add("The 'Refresh Chart' checkbox on the 'Format' window is now never checked for slave charts")
            lst.Add("The 'Range' option on the 'Format' window is now not visible for slave charts")
            Return lst
        End Get
    End Property
    <VersionIndex(6)> Public ReadOnly Property Version20412Changes As List(Of String)
        Get
            Dim lst As New List(Of String)
            lst.Add("Added option to change workspace background color")
            lst.Add("Improved workspace loading and saving efficiency")
            lst.Add("Improved chart dragging - charts now bump into the program edges when resizing")
            lst.Add("The description on orders now shows 'B' and 'S' instead of 'Buy' and 'Sell', respectively")
            lst.Add("The Bid and Ask price lines for the 'Price Lines' analysis technique now snap to the last bar")
            lst.Add("Fixed bug with expiry date picker on the 'Format Symbol' window under certain circumstances")
            lst.Add("Created a beta version of Master/Slave chart systems (File--New--Slave Chart to create a slave chart)")
            Return lst
        End Get
    End Property
    <VersionIndex(5)> Public ReadOnly Property Version20408Changes As List(Of String)
        Get
            Dim lst As New List(Of String)
            lst.Add("Fixed a rare bug when the menu is shown under certain circumstances")
            lst.Add("Changed workspace backgrounds to black")
            Return lst
        End Get
    End Property
    <VersionIndex(4)> Public ReadOnly Property Version20407Changes As List(Of String)
        Get
            Dim lst As New List(Of String)
            lst.Add("Created custom color chooser dialog box")
            lst.Add("Enabled support for transparency in colors")
            lst.Add("Fixed bug with chart snapping up to the top of the screen when resizing under certain circumstances")
            lst.Add("Made changes to the default values of a few settings")
            lst.Add("Improved combo box handling and look for 'Dark', 'Light' and 'Silver' themes")
            lst.Add("Made slight fix to error handling message box on the 'Format Chart' window")
            lst.Add("Added tooltips to color chooser color boxes")
            lst.Add("Fixed bug with editing a color on the color chooser control")
            lst.Add("Improved efficiency for mouse clicking and dragging on charts")
            lst.Add("The 'Symbol Price Minimum Move' input for the 'AutoTrend' analysis technique now calculates at runtime rather than being an input")
            lst.Add("Improved window handling --")
            lst.Add("All dialog boxes are now child windows of other windows")
            lst.Add("All dialog boxes no longer show in the taskbar")
            Return lst
        End Get
    End Property
    <VersionIndex(3)> Public ReadOnly Property Version20404Changes As List(Of String)
        Get
            Dim lst As New List(Of String)
            lst.Add("Added workspace and chart selector menu items")
            lst.Add("Fixed small bug with setting of 'AutoTrend' analysis technique 'Swing RV' value at start")
            lst.Add("Made slight change to 'Time Box' analysis technique start and end line placement")
            Return lst
        End Get
    End Property
    <VersionIndex(2)> Public ReadOnly Property Version20402Changes As List(Of String)
        Get
            Dim lst As New List(Of String)
            lst.Add("Improved order quantity change handling")
            lst.Add("Added 'Default Order Quantity' setting")
            lst.Add("Made minor fix to close button placement")
            lst.Add("Fixed bug in entering non-numeric quantity")
            lst.Add("Removed 'Swing RV' input on the 'AutoTrend' analysis technique")
            Return lst
        End Get
    End Property
    <VersionIndex(1)> Public ReadOnly Property Version20326Changes As List(Of String)
        Get
            Dim lst As New List(Of String)
            lst.Add("Made change to 'AutoTrend' analysis technique gap marker processing")
            lst.Add("Fixed bug in 'TimeBox' analysis technique")
            lst.Add("Removed all bar types except for range bars")
            lst.Add("Fixed issue with regular trading hour option for historical data")
            lst.Add("Made minor change to 'AutoTrend' fill-in feature")
            lst.Add("Added version tracking")
            lst.Add("Added 'New Features' window")
            Return lst
        End Get
    End Property
    Public Class VersionIndex
        Inherits Attribute
        Public Sub New(ByVal versionIndex As Integer)
            Me.VersionIndex = versionIndex
        End Sub
        Public Property VersionIndex As Integer
    End Class
End Module

