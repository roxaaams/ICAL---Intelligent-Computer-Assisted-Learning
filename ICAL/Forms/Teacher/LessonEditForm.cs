﻿/* Copyright 2015-2016 Roxana Teodora Mafteiu-Scai, Diana Fabiola Popa
#
# This file is part of ICAL - Intelligent Computer Assisted Learning.
#
# ICAL - Intelligent Computer Assisted Learning is free software: you can redistribute it and/or modify
# it under the terms of the GNU General Public License as published by
# the Free Software Foundation, either version 3 of the License, or
# (at your option) any later version.
#
# ICAL - Intelligent Computer Assisted Learning is distributed in the hope that it will be useful,
# but WITHOUT ANY WARRANTY; without even the implied warranty of
# MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
# GNU General Public License for more details.
#
# You should have received a copy of the GNU General Public License
# along with ICAL - Intelligent Computer Assisted Learning.  If not, see <http://www.gnu.org/licenses/>.
*/

using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Windows.Forms;
using ICAL_Final.Database;
using ICAL_Final.Datalayer;
using ICAL_Final.Managers;
using ICAL_Final.Resources;

namespace ICAL_Final.Forms.Teacher
{
    public partial class LessonEditForm : Form
    {
        private Action refreshCallback;
        private ICALDatabaseDataSet.LessonsRow lesson;
        private ICALDatabaseDataSet.UsersRow loggedUser;

        /// <summary>
        /// Initializes a new instance of the form
        /// </summary>
        /// <param name="loggedUser"> Used to know which user updates the current lesson </param>
        /// <param name="lesson"> Used to update the selected lesson </param>
        /// <param name="callBack"> Used to call a private method from another form </param>
        public LessonEditForm(ICALDatabaseDataSet.UsersRow loggedUser, ICALDatabaseDataSet.LessonsRow lesson, Action callBack = null)
        {
            InitializeComponent();

            this.lesson = lesson;
            this.loggedUser = loggedUser;
            refreshCallback = callBack;
        }

        /// <summary>
        ///  Shows the lesson on the form
        /// </summary>
        /// <param name="sender"> The sender of the event </param>
        /// <param name="e"> The <see cref="EventArgs"/> instance containing the event data </param>
        private void LessonEditForm_Load(object sender, EventArgs e)
        {
            descriptionLabel.Text += lesson.Id.ToString();
            chapterComboBox.SelectedItem = lesson.IdChapter.ToString();
            titleTextBox.Text = lesson.Title;
            lessonRichTextBox.Rtf = lesson.Lesson;
            if (lesson.Picture != null)
            {
                newPictureBox.Image = ImageManager.ConvertByteArrayToImage(lesson.Picture);
            }
        }

        /// <summary>
        /// Checks if the input is valid
        /// </summary>
        /// <returns> The result of the check </returns>
        protected bool IsInputValid()
        {
            foreach (var control in managementPanel.Controls)
            {
                if (control is TextBox)
                {
                    var input = (control as TextBox).Text;
                    if (input.Length < 1)
                    {
                        NotificationManager.LogException(Strings.InvalidData);
                        return false;
                    }
                }
                else if (control is ComboBox)
                {
                    var input = (control as ComboBox).SelectedItem.ToString();
                    if (input.Length < 1)
                    {
                        NotificationManager.LogException(Strings.InvalidData);
                        return false;
                    }
                }
            }

            if (lessonRichTextBox.Text.Length < 1)
            {
                NotificationManager.LogException(Strings.InvalidData);
                return false;
            }

            return true;
        }

        /// <summary>
        /// Updates the current lesson in the database if the method IsInputValid returns true
        /// and calls back the method from the main form
        /// </summary>
        /// <param name="sender"> The button responsible with serving the intent of saving the changes in the database </param>
        /// <param name="e"> The <see cref="EventArgs"/> instance containing the event data </param>
        private void saveButton_Click(object sender, EventArgs e)
        {
            if (IsInputValid())
            {
                using (var lessonService = new LessonService())
                {
                    lesson.Title = titleTextBox.Text;
                    lesson.Lesson = lessonRichTextBox.Rtf;
                    lesson.UpdatedByIdTeacher = loggedUser.Id;

                    if(newPictureBox.Image != null)
                    {
                        lesson.Picture = ImageManager.ConvertImageToByteArray(newPictureBox.Image, ImageFormat.Bmp);
                    }

                    var saveInstance = lessonService.Update(lesson);
                    if (saveInstance == null)
                    {
                        NotificationManager.LogException(Strings.UpdateError);
                        return;
                    }
                    else
                    {
                        if (refreshCallback != null)
                        {
                            refreshCallback();
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Uploads a new picture for the lesson
        /// </summary>
        /// <param name="sender"> The button responsible with serving the intent of uploading </param>
        /// <param name="e"> The <see cref="EventArgs"/> instance containing the event data </param>
        private void uploadNewPictureButton_Click(object sender, EventArgs e)
        {
            var fileName = ImageManager.UploadImage(Strings.NewPictureForLesson);
            if (fileName != Strings.NoFile)
            {
                newPictureBox.Image = new Bitmap(fileName);
            }
        }

        /// <summary>
        ///  Verifies the user's intent, then deletes the lesson from the database
        /// </summary>
        /// <param name="sender"> The button responsible with serving the intent of deleting </param>
        /// <param name="e"> The <see cref="EventArgs"/> instance containing the event data </param>
        private void deleteButton_Click(object sender, EventArgs e)
        {
            var dialogResult = NotificationManager.ConfirmationDelete(Strings.DeleteConfirmation);
            if (dialogResult)
            {
                using (var lessonService = new LessonService())
                {
                    var saveInstance = lessonService.Delete(lesson.Id);
                    if (saveInstance)
                    {
                        NotificationManager.Alert(Strings.DeleteSucces);
                        Close();
                    }
                    else
                    {
                        NotificationManager.Alert(Strings.UpdateError);
                    }
                }
            }
        }

        /// <summary>
        /// Changes the font of the selected text
        /// </summary>
        /// <param name="sender"> The button responsible with serving the intent of changing the font </param>
        /// <param name="e"> The <see cref="EventArgs"/> instance containing the event data </param>
        private void fontToolStripButton_Click(object sender, EventArgs e)
        {
            var font = LessonManager.SetFont(lessonRichTextBox.SelectionFont);
            lessonRichTextBox.SelectionFont = font;
        }

        /// <summary>
        ///  Changes the font style of the selected text
        /// </summary>
        /// <param name="sender"> The button responsible with serving the intent of changing the font style </param>
        /// <param name="e"> The <see cref="EventArgs"/> instance containing the event data </param>
        private void fontStyleToolStripButton_Click(object sender, EventArgs e)
        {
            var fontStyle = new FontStyle();
            var elementTag = (sender as ToolStripButton).Tag.ToString();
            switch (elementTag)
            {
                case "Bold": fontStyle = FontStyle.Bold; break;
                case "Italic": fontStyle = FontStyle.Italic; break;
                case "Underline": fontStyle = FontStyle.Underline; break;
                case "Strikeout": fontStyle = FontStyle.Strikeout; break;
            }

            if (lessonRichTextBox.SelectionFont.Style.Equals(fontStyle))
            {
                try
                {
                    lessonRichTextBox.SelectionFont = new Font(lessonRichTextBox.SelectionFont, FontStyle.Regular);
                }
                catch (ArgumentException exception)
                {
                    NotificationManager.LogException(exception.ToString());
                }
            }
            else
            {
                try
                {
                    lessonRichTextBox.SelectionFont = new Font(lessonRichTextBox.SelectionFont, fontStyle);
                }
                catch (ArgumentException exception)
                {
                    NotificationManager.LogException(exception.ToString());
                }
            }
        }

        /// <summary>
        /// Changes the color of the selected text
        /// </summary>
        /// <param name="sender"> The button responsible with serving the intent of changing the color </param>
        /// <param name="e"> The <see cref="EventArgs"/> instance containing the event data </param>
        private void colorToolStripButton_Click(object sender, EventArgs e)
        {
            var color = LessonManager.SetColor();
            if (color != null)
            {
                var elementTag = (sender as ToolStripButton).Tag.ToString();
                switch (elementTag)
                {
                    case "Fore color": lessonRichTextBox.SelectionColor = color; break;
                    case "Back color": lessonRichTextBox.SelectionBackColor = color; break;
                }
            }
        }

        /// <summary>
        ///  Changes the align of the selected text
        /// </summary>
        /// <param name="sender"> The button responsible with serving the intent of changing the align </param>
        /// <param name="e"> The <see cref="EventArgs"/> instance containing the event data </param>
        private void alignToolStripButton_Click(object sender, EventArgs e)
        {
            var elementTag = (sender as ToolStripButton).Tag.ToString();
            switch (elementTag)
            {
                case "Left": lessonRichTextBox.SelectionAlignment = HorizontalAlignment.Left; break;
                case "Center": lessonRichTextBox.SelectionAlignment = HorizontalAlignment.Center; break;
                case "Right": lessonRichTextBox.SelectionAlignment = HorizontalAlignment.Right; break;
            }
        }
    }
}
