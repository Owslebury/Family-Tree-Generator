Imports System.Net

Public Class Form1
    Public isDrawing As Boolean = False
    Public startPoint As Point
    Public currentbutton As DraggableButton
    Public Lines As New List(Of (DraggableButton, DraggableButton))
    Public LineButtons As New List(Of LineButtons)
    Public LineAssociatedButton As New List(Of ((DraggableButton, DraggableButton), LineButtons))
    Public itemMoving As Boolean = False
    Public affecteditems As List(Of Button)
    Public ButtonsShowing As Boolean = True
    Public dragbuttons As New List(Of DraggableButton)
    Public unimportantLines As New List(Of (Point, Point))
    Public currentParents As (DraggableButton, DraggableButton)
    Public currentAddButton As New List(Of AddButton)
    'Make sure the generations are in the correct order
    'add ability to remove button/textbox
    'Add ability to add a child without needing another parent, a CHILD button appears when two buttons connected
    'Add a save feature
    'Make the lines connect properly
    'Make a right angle line for children
    Private Sub Form1_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        ' Create the horizontal scrollbar
        Dim hScrollBar As New HScrollBar()
        hScrollBar.Dock = DockStyle.Bottom
        AddHandler hScrollBar.Scroll, AddressOf HScrollBar_Scroll

        ' Create the vertical scrollbar
        Dim vScrollBar As New VScrollBar()
        vScrollBar.Dock = DockStyle.Right
        AddHandler vScrollBar.Scroll, AddressOf VScrollBar_Scroll
        vScrollBar.LargeChange = 500
        hScrollBar.LargeChange = 500
        vScrollBar.Maximum = Me.Height * 3
        hScrollBar.Maximum = Me.Width * 3

        ' Add the scrollbars to the form
        Me.Controls.Add(hScrollBar)
        Me.Controls.Add(vScrollBar)

        ' Move all the controls and custom classes along with the scrollbars
        For Each ctrl As Control In Me.Controls
            If Not TypeOf ctrl Is HScrollBar And Not TypeOf ctrl Is VScrollBar Then
                ctrl.Location = New Point(ctrl.Location.X - hScrollBar.Value, ctrl.Location.Y - vScrollBar.Value)
            End If
        Next
    End Sub

    Private Sub HScrollBar_Scroll(sender As Object, e As ScrollEventArgs)
        ' Move all the controls and custom classes horizontally along with the scrollbar
        For Each ctrl As Control In Me.Controls
            If Equals(ctrl, Button1) = False And Equals(ctrl, Button2) = False Then
                If Not TypeOf ctrl Is HScrollBar And Not TypeOf ctrl Is VScrollBar Then
                    ctrl.Location = New Point(ctrl.Location.X + e.OldValue - e.NewValue, ctrl.Location.Y)
                    Invalidate()
                End If
            End If
        Next
    End Sub

    Private Sub VScrollBar_Scroll(sender As Object, e As ScrollEventArgs)
        ' Move all the controls and custom classes vertically along with the scrollbar
        For Each ctrl As Control In Me.Controls
            If Equals(ctrl, Button1) = False And Equals(ctrl, Button2) = False Then
                If Not TypeOf ctrl Is HScrollBar And Not TypeOf ctrl Is VScrollBar Then
                    ctrl.Location = New Point(ctrl.Location.X, ctrl.Location.Y + e.OldValue - e.NewValue)
                    Invalidate()
                End If
            End If
        Next
    End Sub

    Private Sub Button1_Click(sender As Object, e As EventArgs) Handles Button1.Click
        Dim button As New DraggableButton()
        dragbuttons.Add(button)
        Dim textbox As New DraggableTextbox()
        Dim remove As New RemoveFamilyMemberButton

        currentbutton = button
        button.myTextbox(textbox)
        textbox.myButton(button)
        textbox.remove = remove
        remove.myFamily = textbox
        Me.Controls.Add(textbox)
        Me.Controls.Add(button)
        Me.Controls.Add(remove)
        isDrawing = False
        startPoint = middlelocation(button)
        remove.Location = New Point(button.Location.X + 50, button.Location.Y)
        textbox.buttonClicked = True
    End Sub
    Private Sub Form1_RightClick(sender As Object, e As MouseEventArgs) Handles Me.MouseDoubleClick
        isDrawing = False
        Me.Invalidate()
    End Sub
    Public Function getAssociatedlinebutton(line As (DraggableButton, DraggableButton))
        For Each myline In LineAssociatedButton
            If Equals(myline.Item1, line) Then
                Return myline.Item2
            End If
        Next
        'this returns the line button for a given line

    End Function
    Public Sub removeLine(mybutton As DraggableButton)
        Dim linestoremove As New List(Of (DraggableButton, DraggableButton))
        Dim linebuttonstoremove As New List(Of LineButtons)
        For Each line In Lines
            If Equals(line.Item1, mybutton) Or Equals(line.Item2, mybutton) Then
                linebuttonstoremove.Add(getAssociatedlinebutton(line))
                linestoremove.Add(line)
            End If
        Next
        For Each line In linestoremove
            Lines.Remove(line)
        Next

        For Each member In linebuttonstoremove
            If Equals(member, Nothing) = False Then
                member.btn2.removeAll()
                member.Remove()
                LineButtons.Remove(member)
            End If
        Next
        Invalidate()
    End Sub
    Private Sub Form1_Resize(sender As Object, e As EventArgs) Handles Me.Resize
        Button1.Left = (Me.ClientSize.Width - Button1.Width) / 2
        Button1.Top = Me.ClientSize.Height - Button1.Height - 20
        Button2.Top = Me.ClientSize.Height - Button1.Height - 20
    End Sub
    Private Sub Form1_MouseMove(sender As Object, e As MouseEventArgs) Handles Me.MouseMove
        If isDrawing Then
            Me.Invalidate()
        End If
    End Sub
    Private Sub Form1_Paint(sender As Object, e As PaintEventArgs) Handles Me.Paint
        If isDrawing Then
            Dim endPoint As Point = Me.PointToClient(MousePosition)
            e.Graphics.DrawLine(Pens.Black, startPoint, endPoint)
        End If
    End Sub
    Private Sub DrawLine(startPoint As Point, endPoint As Point)
        Dim g As Graphics = Me.CreateGraphics()
        g.DrawLine(Pens.Black, startPoint, New Point(endPoint.X, startPoint.Y))
        g.DrawLine(Pens.Black, New Point(endPoint.X, startPoint.Y), endPoint)
    End Sub
    Private Sub PermenantLines(sender As Object, e As PaintEventArgs) Handles Me.Paint
        Dim midpoints As New List(Of Point)
        If itemMoving = True Then
            For Each item In Lines
                If item.Item1.isLineButton = False And item.Item2.isLineButton = False Then
                    e.Graphics.DrawLine(Pens.Black, middlelocation(item.Item1), middlelocation(item.Item2))
                Else
                    If item.Item1.isLineButton Then
                        DrawLine(middlelocation(item.Item1), middlelocation(item.Item2))
                    Else
                        DrawLine(middlelocation(item.Item2), middlelocation(item.Item1))
                    End If

                End If
                If item.Item1.isLineButton = False Then
                    If ButtonsShowing = False Then
                        e.Graphics.DrawLine(Pens.Black, middlelocation(item.Item1), middlelocationtextbox(item.Item1.associatedTextbox))
                    End If
                End If
                If item.Item2.isLineButton = False Then
                    If ButtonsShowing = False Then
                        e.Graphics.DrawLine(Pens.Black, middlelocation(item.Item2), middlelocationtextbox(item.Item2.associatedTextbox))
                    End If
                End If
                If item.Item1.isLineButton = False And item.Item2.isLineButton = False Then
                    midpoints.Add(New Point((middlelocation(item.Item1).X + middlelocation(item.Item2).X) / 2, (middlelocation(item.Item1).Y + middlelocation(item.Item2).Y) / 2))
                End If
            Next
            For i = 0 To midpoints.Count - 1
                If LineButtons(i).mode = True Then
                    LineButtons(i).updateLocation(New Point(midpoints(i).X, midpoints(i).Y - 10))
                    startPoint = currentbutton.Location
                    'For +line button, makes sure the selection line follows it
                End If
            Next
        End If
    End Sub
    Public Function middlelocation(btn As Button)
        Dim middleX As Integer
        Dim middleY As Integer

        middleX = btn.Left + btn.Width
        middleY = btn.Top + btn.Height / 2
        Return New Point(middleX, middleY)
    End Function
    Public Function middlelocationtextbox(text As TextBox)
        Dim middleX As Integer
        Dim middleY As Integer

        middleX = text.Left + text.Width / 2
        middleY = text.Top + text.Height / 2
        Return New Point(middleX, middleY)
    End Function

    Private Sub Button2_Click(sender As Object, e As EventArgs) Handles Button2.Click
        If Button2.Text(0) = "H" Then
            Button2.Text = "Show Buttons"
            ButtonsShowing = False
            For Each item In LineButtons
                item.hide()
            Next
            For Each item In dragbuttons
                item.Hide()
                item.associatedTextbox.remove.Hide()
            Next
        Else
            Button2.Text = "Hide Buttons"
            ButtonsShowing = True
            For Each item In LineButtons
                item.show()
            Next
            For Each item In dragbuttons
                item.Show()
                item.associatedTextbox.remove.Show()
            Next

        End If
    End Sub
End Class

Public Class LineButtons

    Public btn1 As AddButton
    Public btn2 As RemoveButton
    Private myLocation As Point
    Private myReference As (DraggableButton, DraggableButton)
    Public mode As Boolean
    Public midlocation As Point


    Public Sub New(reference As (DraggableButton, DraggableButton), location As Point, onOff As Boolean)
        reference.Item1.linebuttons.Add(Me)
        reference.Item2.linebuttons.Add(Me)
        mode = onOff
        If onOff = True Then
            myLocation = location
            myReference = reference
            'Create the first button
            btn1 = New AddButton
            btn1.Size = New Size(20, 20)
            btn1.Location = New Point(location.X - 10, location.Y)
            Form1.Controls.Add(btn1)

            'Create the second button
            btn2 = New RemoveButton
            btn2.line.Add(myReference)
            btn2.linebutton = Me
            btn2.Size = New Size(20, 20)
            btn2.Location = New Point(location.X + 10, location.Y)
            Form1.Controls.Add(btn2)
            btn1.associatedRemove = btn2
            If Form1.ButtonsShowing = False Then
                hide()
            Else
                show()
            End If
        End If
    End Sub
    Public Sub updateLocation(location As Point)
        btn1.Location = New Point(location.X - 10, location.Y)
        btn2.Location = New Point(location.X + 10, location.Y)
    End Sub
    Public Function getreference()
        Return myReference
    End Function
    Public Sub hide()
        Form1.ButtonsShowing = False
        If Equals(btn1, Nothing) = False And Equals(btn2, Nothing) = False Then
            btn1.Hide()
            btn2.Hide()
        End If

    End Sub
    Public Sub show()
        Form1.ButtonsShowing = True
        If Equals(btn1, Nothing) = False And Equals(btn2, Nothing) = False Then
            btn1.Show()
            btn2.Show()
        End If
    End Sub
    Public Sub Remove()
        Form1.Controls.Remove(btn1)
        Form1.Controls.Remove(btn2)
        Form1.LineButtons.Remove(Me)
        Form1.Invalidate()
    End Sub
End Class
Public Class RemoveButton
    Inherits DraggableButton
    Public line As New List(Of (DraggableButton, DraggableButton))
    Public linebutton As LineButtons
    Public Sub New()
        Me.Text = "-"
        isLineButton = True
    End Sub
    Private Sub RemoveButton_Click(sender As Object, e As EventArgs) Handles Me.Click
        Form1.isDrawing = False
        For Each item In line
            Form1.Lines.Remove(item)
            item.Item1.HusbandsAndWives.Remove(item.Item2)
            item.Item2.HusbandsAndWives.Remove(item.Item1)
        Next
        linebutton.Remove()
    End Sub
    Public Sub removeAll()
        Form1.isDrawing = False
        For Each item In line
            Form1.Lines.Remove(item)
            item.Item1.HusbandsAndWives.Remove(item.Item2)
            item.Item2.HusbandsAndWives.Remove(item.Item1)
        Next
        linebutton.Remove()
    End Sub
End Class
Public Class AddButton
    Inherits DraggableButton
    Public line As (DraggableButton, DraggableButton)
    Public associatedRemove As RemoveButton
    Public Sub New()
        Me.Text = "+"
        isLineButton = True
        Form1.currentAddButton.Add(Me)
    End Sub
    Private Sub AddButton_Click(sender As Object, e As EventArgs) Handles Me.Click
        If Form1.isDrawing = False And Equals(Form1.currentbutton, Me) = False Then
            Form1.isDrawing = True
        End If
        If Form1.currentAddButton.Contains(Me) = False Then
            Form1.currentAddButton.Add(Me)
        End If
        Form1.currentParents = line
        Form1.startPoint = Form1.middlelocation(Me)
        Form1.currentbutton = Me
    End Sub

End Class
Public Class RemoveFamilyMemberButton
    Inherits DraggableButton
    Public myFamily As DraggableTextbox
    Public Sub New()
        Me.SendToBack()
        Me.Text = "-"
    End Sub
    Private Sub RemoveFamily_Click(sender As Object, e As EventArgs) Handles Me.MouseDown
        Form1.isDrawing = False
        For Each person In myFamily.associatedButton.HusbandsAndWives
            If Equals(person.associatedTextbox.remove, Me) Then
                person.HusbandsAndWives.Remove(Me.associatedTextbox.associatedButton)
            End If
        Next
        myFamily.associatedButton.HusbandsAndWives.Clear()
        Form1.removeLine(myFamily.associatedButton)
        Form1.Controls.Remove(myFamily.associatedButton)
        Form1.Controls.Remove(myFamily)
        Form1.Controls.Remove(Me)
    End Sub
End Class


Public Class DraggableTextbox
    Inherits TextBox
    Public buttonClicked As Boolean = False
    Private mouseLocation As Point
    Public associatedButton As DraggableButton
    Public remove As RemoveFamilyMemberButton

    Public Sub New()
        Me.AllowDrop = True
        Me.BorderStyle = BorderStyle.FixedSingle
        Me.Size = New Size(100, 70)
        Me.Multiline = True
    End Sub
    Public Sub myButton(ByRef butto As Button)
        associatedButton = butto
    End Sub
    Private Sub DraggableTextbox_MouseDown(sender As Object, e As MouseEventArgs) Handles Me.MouseDown
        mouseLocation = e.Location
        Form1.itemMoving = False
    End Sub

    Private Sub DraggableTextbox_MouseMove(sender As Object, e As MouseEventArgs) Handles Me.MouseMove
        If e.Button = MouseButtons.Left Then
            Form1.Invalidate()
            Form1.itemMoving = True
            Me.Left = e.X + Me.Left - mouseLocation.X
            Me.Top = e.Y + Me.Top - mouseLocation.Y

            Dim buttonpoint As New Point(Me.Location.X, Me.Location.Y + 60)
            associatedButton.Location = buttonpoint
            remove.Location = New Point(buttonpoint.X + 50, buttonpoint.Y)
            updateLocation(buttonpoint, Me)
            If Equals(Form1.currentbutton, associatedButton) And buttonClicked = True Then
                Form1.startPoint = buttonpoint
            End If
        End If
    End Sub
    Public Sub updateLocation(buttonpoint As Point, notThisTextbox As TextBox)

        For Each member In Me.associatedButton.HusbandsAndWives
            member.associatedTextbox.Location = New Point(member.associatedTextbox.Location.X, buttonpoint.Y - 60)
            member.associatedTextbox.remove.Location = New Point(member.associatedTextbox.Location.X + 50, buttonpoint.Y)
            member.Location = New Point(member.Location.X, buttonpoint.Y)
            If Equals(member.associatedTextbox, notThisTextbox) = False Then
                member.associatedTextbox.updateLocation(buttonpoint, Me)
            End If
        Next
        'this is where family members on a generation keep on the same y axis
    End Sub

End Class

Public Class DraggableButton
    Inherits Button
    Private mouseLocation As Point
    Public associatedTextbox As DraggableTextbox
    Private _mouseDown As Boolean
    Private _startPoint As Point
    Public Clicked As Boolean
    Public isLineButton As Boolean = False
    Public Parents As New List(Of DraggableButton)
    Public HusbandsAndWives As New List(Of DraggableButton)
    Public linebuttons As New List(Of LineButtons)
    Public Children As New List(Of DraggableButton)
    Public myLineButtons As New List(Of LineButtons)
    Public lines As New List(Of (DraggableButton, DraggableButton))

    Public Sub New()
        Me.AllowDrop = True
        Me.Size = New Size(50, 30)
        Me.Text = "+"
        If Form1.ButtonsShowing = False Then
            hidebutton()
        End If
    End Sub
    Public Sub myTextbox(ByRef Textbox As DraggableTextbox)
        associatedTextbox = Textbox
        Dim initialLocation As New Point(associatedTextbox.Location.X, associatedTextbox.Location.Y + 60)
        Me.Location = initialLocation
    End Sub

    Private Sub DraggableButton_MouseDown(sender As Object, e As MouseEventArgs) Handles Me.MouseDown
        Clicked = True
        Dim form As Form = Me.FindForm()
        Form1.startPoint = Form1.middlelocation(Me)
        If Equals(associatedTextbox, Nothing) = False Then
            associatedTextbox.buttonClicked = True
        End If
        If Form1.isDrawing And Form1.currentbutton.Clicked = True Then
            'Makes sure that if two points are connected, the line that follows the mouse stops
            If Form1.Lines.Contains((Form1.currentbutton, Me)) = False And Form1.Lines.Contains((Me, Form1.currentbutton)) = False And Equals(Form1.currentbutton, Me) = False Then
                Dim midPoint As New Point((Form1.middlelocation(Form1.currentbutton).X + Form1.middlelocation(Me).X) / 2, (Form1.middlelocation(Form1.currentbutton).Y + Form1.middlelocation(Me).Y) / 2)
                If Form1.currentbutton.isLineButton = False And Me.isLineButton = False Then
                    Form1.currentbutton.HusbandsAndWives.Add(Me)
                    HusbandsAndWives.Add(Form1.currentbutton)
                    Dim linebutton As New LineButtons((Form1.currentbutton, Me), midPoint, True)
                    myLineButtons.Add(linebutton)
                    Form1.Lines.Add((Form1.currentbutton, Me))
                    Form1.LineButtons.Add(linebutton)
                    Form1.LineAssociatedButton.Add(((Form1.currentbutton, Me), linebutton))
                Else
                    Dim linebutton As LineButtons = Form1.getAssociatedlinebutton((Form1.currentbutton, Me))
                    Dim invalid As Boolean = False
                    If Me.isLineButton And Form1.currentbutton.isLineButton Then
                        invalid = True
                    End If
                    If Form1.currentbutton.isLineButton Then
                        For i = 0 To Form1.Lines.Count - 1
                            If Equals(Form1.Lines(i).Item1, Form1.currentbutton) Or Equals(Form1.Lines(i).Item2, Form1.currentbutton) Then
                                If Equals(Form1.getAssociatedlinebutton(Form1.Lines(i)), Nothing) = False Then
                                    If Form1.getAssociatedlinebutton(Form1.Lines(i)).btn1.location = Form1.currentbutton.Location Then
                                        invalid = True
                                    End If
                                End If
                            End If
                        Next
                    Else
                        For i = 0 To Form1.Lines.Count - 1
                            If Equals(Form1.Lines(i).Item1, Me) Or Equals(Form1.Lines(i).Item2, Me) Then
                                If Equals(Form1.getAssociatedlinebutton(Form1.Lines(i)), Nothing) = False Then
                                    If Form1.getAssociatedlinebutton(Form1.Lines(i)).btn1.location = Me.Location Then
                                        invalid = True
                                    End If
                                End If
                            End If
                        Next
                    End If
                    'this ensures that a line button cannot be added to its own family member
                    If invalid = False Then
                        'Stops add button inbetween parents being added to the same parent
                        Form1.Lines.Add((Form1.currentbutton, Me))
                        Form1.currentAddButton.Clear()
                        If Me.isLineButton = True Then
                            Form1.currentAddButton.Add(Me)
                        Else
                            Form1.currentAddButton.Add(Form1.currentbutton)
                        End If
                        For Each item In Form1.currentAddButton
                            item.associatedRemove.line.Add((Form1.currentbutton, Me))
                        Next
                    End If
                End If
            End If
            'adds to the list of permenant lines if the connection is not already made
            Form1.isDrawing = False
            Clicked = False
            Form1.currentbutton.Clicked = False
        Else
            Form1.isDrawing = True
        End If
        Form1.currentbutton = Me

    End Sub
    Private Sub Form1_MouseMove(sender As Object, e As MouseEventArgs) Handles Me.MouseMove
        'Form1.Invalidate()
    End Sub
    Public Sub hidebutton()
        Me.Hide()
    End Sub
    Public Sub showbutton()
        Me.Show()
    End Sub
End Class

