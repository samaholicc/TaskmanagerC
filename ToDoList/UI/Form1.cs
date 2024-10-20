using System;
using System.Collections.Generic;
using System.Windows.Forms;
using MySql.Data.MySqlClient;
using static ToDoItem;

namespace ToDoList
{
    public partial class Form1 : Form
    {
        private ToDoDatabaseManager dbManager;  // Gestionnaire de la base de donn�es
        private List<ToDoItem> Tasks = new List<ToDoItem>();  // Liste des t�ches

        public Form1()
        {
            InitializeComponent();

            // Initialisation des �v�nements
            checkBoxListToDo.ItemCheck += new ItemCheckEventHandler(this.checkBoxListToDo_ItemCheck);
            checkBoxListToDo.MouseDown += new MouseEventHandler(this.checkBoxListToDo_MouseDown);

            // Initialisation du gestionnaire de base de donn�es avec une cha�ne de connexion
            string connectionString = "Server=localhost;Database=task;User ID=root;Password=samia;";
            dbManager = new ToDoDatabaseManager(connectionString);

            // Param�trage du mode de dessin personnalis� pour la liste des t�ches effectu�es
            listDone.DrawMode = DrawMode.OwnerDrawFixed;
            listDone.DrawItem += new DrawItemEventHandler(listDone_DrawItem);
        }

        // �v�nement de chargement du formulaire
        private void Form1_Load(object sender, EventArgs e)
        {
            // Lecture des t�ches � partir de la base de donn�es
            this.Tasks = dbManager.ReadToDoList();
            this.FillLists();  // Remplissage des listes de t�ches dans l'interface utilisateur

            // Notification � l'utilisateur
            this.notifyIcon1.BalloonTipText = "Les t�ches ont �t� charg�es.";
            this.notifyIcon1.ShowBalloonTip(2000);
        }

        // M�thode pour remplir les listes de t�ches (en attente et termin�es)
        private void FillLists()
        {
            // Effacer les �l�ments existants dans les listes
            checkBoxListToDo.Items.Clear();
            listDone.Items.Clear();

            // Parcourir toutes les t�ches en m�moire
            foreach (ToDoItem task in Tasks)
            {
                if (!task.IsDone)
                {
                    checkBoxListToDo.Items.Add(task);  // Ajouter aux t�ches en attente
                }
                else
                {
                    listDone.Items.Add(task);  // Ajouter aux t�ches termin�es
                }
            }
        }

        // Gestion du dessin des �l�ments de la liste des t�ches termin�es avec des couleurs en fonction de la priorit�
        private void listDone_DrawItem(object sender, DrawItemEventArgs e)
        {
            if (e.Index < 0) return;

            // R�cup�ration de la t�che associ�e
            ToDoItem task = (ToDoItem)listDone.Items[e.Index];

            // Dessiner l'arri�re-plan
            e.DrawBackground();

            // Choisir une couleur en fonction de la priorit� de la t�che
            Color priorityColor;
            switch (task.Priority)
            {
                case TaskPriority.High:
                    priorityColor = Color.Red;  // Priorit� �lev�e - Rouge
                    break;
                case TaskPriority.Medium:
                    priorityColor = Color.Orange;  // Priorit� moyenne - Orange
                    break;
                case TaskPriority.Low:
                    priorityColor = Color.Green;  // Priorit� basse - Vert
                    break;
                default:
                    priorityColor = Color.Black;  // Couleur par d�faut
                    break;
            }

            // Dessiner le texte de la t�che avec la couleur appropri�e
            using (Brush textBrush = new SolidBrush(priorityColor))
            {
                e.Graphics.DrawString(task.ToString(), e.Font, textBrush, e.Bounds);
            }

            // Dessiner le rectangle de focus si n�cessaire
            e.DrawFocusRectangle();
        }

        // M�thode pour ajouter une nouvelle t�che
        private void btnAddItem_Click(object sender, EventArgs e)
        {
            // R�cup�rer le texte de la t�che � partir du champ de saisie
            string newTaskText = txtNewTask.Text.Trim();

            // V�rifier si une priorit� est s�lectionn�e
            if (comboBox1.SelectedItem == null)
            {
                MessageBox.Show("Veuillez s�lectionner une priorit� pour la t�che.", "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // Convertir la priorit� s�lectionn�e en �num�ration TaskPriority
            TaskPriority selectedPriority;
            if (Enum.TryParse(comboBox1.SelectedItem.ToString(), out selectedPriority))
            {
                if (!string.IsNullOrWhiteSpace(newTaskText))
                {
                    // Cr�er un nouvel objet ToDoItem pour la t�che
                    ToDoItem newTask = new ToDoItem()
                    {
                        ID = Guid.NewGuid(),
                        ItemText = newTaskText,
                        IsDone = false,  // Par d�faut, la t�che n'est pas termin�e
                        Priority = selectedPriority,
                        DoneDate = null  // Aucune date de fin pour les t�ches en attente
                    };

                    // Ajouter la t�che � la liste en m�moire et � la base de donn�es
                    this.Tasks.Add(newTask);
                    dbManager.AddToDoItem(newTask);

                    // Ajouter la t�che � l'interface utilisateur (liste des t�ches en attente)
                    checkBoxListToDo.Items.Add(newTask);

                    // Effacer le champ de saisie et notifier l'utilisateur
                    this.txtNewTask.Text = string.Empty;
                    this.notifyIcon1.BalloonTipText = "Nouvelle t�che ajout�e.";
                    this.notifyIcon1.ShowBalloonTip(2000);
                }
                else
                {
                    MessageBox.Show("Veuillez entrer une description de la t�che.", "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            else
            {
                MessageBox.Show("Veuillez s�lectionner une priorit� valide.", "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // Gestion des modifications des cases � cocher (marquage comme termin�/non termin�)
        private void checkBoxListToDo_ItemCheck(object sender, ItemCheckEventArgs e)
        {
            this.BeginInvoke((MethodInvoker)delegate
            {
                ToDoItem task = (ToDoItem)checkBoxListToDo.Items[e.Index];

                // Si la case est coch�e, marquer la t�che comme termin�e
                if (e.NewValue == CheckState.Checked)
                {
                    task.IsDone = true;
                    task.DoneDate = DateTime.Now;
                }
                else
                {
                    task.IsDone = false;
                    task.DoneDate = null;
                }

                dbManager.UpdateToDoItem(task);  // Mettre � jour la t�che dans la base de donn�es
                this.FillLists();  // Rafra�chir les listes
            });
        }

        // Gestion de la s�lection des �l�ments sans modifier l'�tat des cases � cocher
        private void checkBoxListToDo_MouseDown(object sender, MouseEventArgs e)
        {
            int index = checkBoxListToDo.IndexFromPoint(e.Location);

            if (index != ListBox.NoMatches)
            {
                Rectangle itemBounds = checkBoxListToDo.GetItemRectangle(index);
                Rectangle checkboxBounds = new Rectangle(itemBounds.X, itemBounds.Y, 16, itemBounds.Height);

                // Si le clic n'est pas sur la case � cocher, seulement s�lectionner l'�l�ment
                if (!checkboxBounds.Contains(e.Location))
                {
                    checkBoxListToDo.SelectedIndex = index;
                    e = null;  // Emp�cher la modification de l'�tat de la case � cocher
                }
            }
        }

        // M�thode pour �diter une t�che
        private void btnEditItem_Click(object sender, EventArgs e)
        {
            if (checkBoxListToDo.SelectedItem != null)
            {
                ToDoItem selectedTask = (ToDoItem)checkBoxListToDo.SelectedItem;
                string newTaskText = txtNewTask.Text.Trim();

                if (!string.IsNullOrWhiteSpace(newTaskText))
                {
                    selectedTask.ItemText = newTaskText;
                    dbManager.UpdateToDoItem(selectedTask);
                    this.FillLists();
                    this.txtNewTask.Text = string.Empty;
                    this.notifyIcon1.BalloonTipText = "T�che modifi�e.";
                    this.notifyIcon1.ShowBalloonTip(2000);
                }
                else
                {
                    MessageBox.Show("Veuillez entrer un texte valide pour modifier la t�che.", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            else
            {
                MessageBox.Show("Veuillez s�lectionner une t�che � modifier.", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        // M�thode pour supprimer une t�che
        private void btnDeleteItem_Click(object sender, EventArgs e)
        {
            if (checkBoxListToDo.SelectedItem != null)
            {
                ToDoItem selectedTask = (ToDoItem)checkBoxListToDo.SelectedItem;

                var confirmResult = MessageBox.Show("�tes-vous s�r de vouloir supprimer cette t�che ?",
                                                     "Confirmer la suppression",
                                                     MessageBoxButtons.YesNo,
                                                     MessageBoxIcon.Warning);
                if (confirmResult == DialogResult.Yes)
                {
                    Tasks.Remove(selectedTask);
                    dbManager.DeleteToDoItem(selectedTask);
                    this.FillLists();
                    this.notifyIcon1.BalloonTipText = "T�che supprim�e.";
                    this.notifyIcon1.ShowBalloonTip(2000);
                }
            }
            else
            {
                MessageBox.Show("Veuillez s�lectionner une t�che � supprimer.", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }
        private void btnCopy_Click(object sender, EventArgs e)
        {
            // V�rifier si un texte est s�lectionn� dans la zone de texte
            if (!string.IsNullOrEmpty(txtNewTask.SelectedText))
            {
                // Copier le texte s�lectionn� dans le presse-papier
                Clipboard.SetText(txtNewTask.SelectedText);
            }
            else
            {
                // Si aucun texte n'est s�lectionn�, copier tout le texte
                Clipboard.SetText(txtNewTask.Text);
            }

            // Donner le focus � la zone de texte
            txtNewTask.Focus();
        }
        private void btnCut_Click(object sender, EventArgs e)
        {
            // V�rifier si un texte est s�lectionn� dans la zone de texte
            if (!string.IsNullOrEmpty(txtNewTask.SelectedText))
            {
                // Copier le texte s�lectionn� dans le presse-papier
                Clipboard.SetText(txtNewTask.SelectedText);

                // Supprimer le texte s�lectionn�
                txtNewTask.Text = txtNewTask.Text.Remove(txtNewTask.SelectionStart, txtNewTask.SelectionLength);
            }
            else
            {
                // Si aucun texte n'est s�lectionn�, couper tout le texte
                Clipboard.SetText(txtNewTask.Text);
                txtNewTask.Text = string.Empty;
            }

            // Donner le focus � la zone de texte
            txtNewTask.Focus();
        }
        private void btnPaste_Click(object sender, EventArgs e)
        {
            // V�rifier si le presse-papier contient du texte
            if (Clipboard.ContainsText())
            {
                // Coller le texte � l'emplacement du curseur
                int selectionStart = txtNewTask.SelectionStart;
                txtNewTask.Text = txtNewTask.Text.Insert(selectionStart, Clipboard.GetText());

                // Ajuster le curseur � la fin du texte coll�
                txtNewTask.SelectionStart = selectionStart + Clipboard.GetText().Length;
            }

            // Donner le focus � la zone de texte
            txtNewTask.Focus();
        }
        private void menuExit_Click(object sender, EventArgs e)
        {
            // Afficher un message de confirmation avant de quitter l'application
            var confirmResult = MessageBox.Show("�tes-vous s�r de vouloir quitter ?", "Quitter", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            // Si l'utilisateur confirme, fermer l'application
            if (confirmResult == DialogResult.Yes)
            {
                Application.Exit();
            }
        }
        private void menuNewItem_Click(object sender, EventArgs e)
        {
            // Simuler un clic sur le bouton d'ajout de t�che
            btnAddItem_Click(sender, e);
        }
        private void menuDeleteItem_Click(object sender, EventArgs e)
        {
            // Simuler un clic sur le bouton de suppression de t�che
            btnDeleteItem_Click(sender, e);
        }
        private void menuEditItem_Click(object sender, EventArgs e)
        {
            // Simuler un clic sur le bouton de modification de t�che
            btnEditItem_Click(sender, e);
        }

    }
}
