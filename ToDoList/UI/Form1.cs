using System;
using System.Collections.Generic;
using System.Windows.Forms;
using MySql.Data.MySqlClient;
using static ToDoItem;

namespace ToDoList
{
    public partial class Form1 : Form
    {
        private ToDoDatabaseManager dbManager;  // Gestionnaire de la base de données
        private List<ToDoItem> Tasks = new List<ToDoItem>();  // Liste des tâches

        public Form1()
        {
            InitializeComponent();

            // Initialisation des événements
            checkBoxListToDo.ItemCheck += new ItemCheckEventHandler(this.checkBoxListToDo_ItemCheck);
            checkBoxListToDo.MouseDown += new MouseEventHandler(this.checkBoxListToDo_MouseDown);

            // Initialisation du gestionnaire de base de données avec une chaîne de connexion
            string connectionString = "Server=localhost;Database=task;User ID=root;Password=samia;";
            dbManager = new ToDoDatabaseManager(connectionString);

            // Paramétrage du mode de dessin personnalisé pour la liste des tâches effectuées
            listDone.DrawMode = DrawMode.OwnerDrawFixed;
            listDone.DrawItem += new DrawItemEventHandler(listDone_DrawItem);
        }

        // Événement de chargement du formulaire
        private void Form1_Load(object sender, EventArgs e)
        {
            // Lecture des tâches à partir de la base de données
            this.Tasks = dbManager.ReadToDoList();
            this.FillLists();  // Remplissage des listes de tâches dans l'interface utilisateur

            // Notification à l'utilisateur
            this.notifyIcon1.BalloonTipText = "Les tâches ont été chargées.";
            this.notifyIcon1.ShowBalloonTip(2000);
        }

        // Méthode pour remplir les listes de tâches (en attente et terminées)
        private void FillLists()
        {
            // Effacer les éléments existants dans les listes
            checkBoxListToDo.Items.Clear();
            listDone.Items.Clear();

            // Parcourir toutes les tâches en mémoire
            foreach (ToDoItem task in Tasks)
            {
                if (!task.IsDone)
                {
                    checkBoxListToDo.Items.Add(task);  // Ajouter aux tâches en attente
                }
                else
                {
                    listDone.Items.Add(task);  // Ajouter aux tâches terminées
                }
            }
        }

        // Gestion du dessin des éléments de la liste des tâches terminées avec des couleurs en fonction de la priorité
        private void listDone_DrawItem(object sender, DrawItemEventArgs e)
        {
            if (e.Index < 0) return;

            // Récupération de la tâche associée
            ToDoItem task = (ToDoItem)listDone.Items[e.Index];

            // Dessiner l'arrière-plan
            e.DrawBackground();

            // Choisir une couleur en fonction de la priorité de la tâche
            Color priorityColor;
            switch (task.Priority)
            {
                case TaskPriority.High:
                    priorityColor = Color.Red;  // Priorité élevée - Rouge
                    break;
                case TaskPriority.Medium:
                    priorityColor = Color.Orange;  // Priorité moyenne - Orange
                    break;
                case TaskPriority.Low:
                    priorityColor = Color.Green;  // Priorité basse - Vert
                    break;
                default:
                    priorityColor = Color.Black;  // Couleur par défaut
                    break;
            }

            // Dessiner le texte de la tâche avec la couleur appropriée
            using (Brush textBrush = new SolidBrush(priorityColor))
            {
                e.Graphics.DrawString(task.ToString(), e.Font, textBrush, e.Bounds);
            }

            // Dessiner le rectangle de focus si nécessaire
            e.DrawFocusRectangle();
        }

        // Méthode pour ajouter une nouvelle tâche
        private void btnAddItem_Click(object sender, EventArgs e)
        {
            // Récupérer le texte de la tâche à partir du champ de saisie
            string newTaskText = txtNewTask.Text.Trim();

            // Vérifier si une priorité est sélectionnée
            if (comboBox1.SelectedItem == null)
            {
                MessageBox.Show("Veuillez sélectionner une priorité pour la tâche.", "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // Convertir la priorité sélectionnée en énumération TaskPriority
            TaskPriority selectedPriority;
            if (Enum.TryParse(comboBox1.SelectedItem.ToString(), out selectedPriority))
            {
                if (!string.IsNullOrWhiteSpace(newTaskText))
                {
                    // Créer un nouvel objet ToDoItem pour la tâche
                    ToDoItem newTask = new ToDoItem()
                    {
                        ID = Guid.NewGuid(),
                        ItemText = newTaskText,
                        IsDone = false,  // Par défaut, la tâche n'est pas terminée
                        Priority = selectedPriority,
                        DoneDate = null  // Aucune date de fin pour les tâches en attente
                    };

                    // Ajouter la tâche à la liste en mémoire et à la base de données
                    this.Tasks.Add(newTask);
                    dbManager.AddToDoItem(newTask);

                    // Ajouter la tâche à l'interface utilisateur (liste des tâches en attente)
                    checkBoxListToDo.Items.Add(newTask);

                    // Effacer le champ de saisie et notifier l'utilisateur
                    this.txtNewTask.Text = string.Empty;
                    this.notifyIcon1.BalloonTipText = "Nouvelle tâche ajoutée.";
                    this.notifyIcon1.ShowBalloonTip(2000);
                }
                else
                {
                    MessageBox.Show("Veuillez entrer une description de la tâche.", "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            else
            {
                MessageBox.Show("Veuillez sélectionner une priorité valide.", "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // Gestion des modifications des cases à cocher (marquage comme terminé/non terminé)
        private void checkBoxListToDo_ItemCheck(object sender, ItemCheckEventArgs e)
        {
            this.BeginInvoke((MethodInvoker)delegate
            {
                ToDoItem task = (ToDoItem)checkBoxListToDo.Items[e.Index];

                // Si la case est cochée, marquer la tâche comme terminée
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

                dbManager.UpdateToDoItem(task);  // Mettre à jour la tâche dans la base de données
                this.FillLists();  // Rafraîchir les listes
            });
        }

        // Gestion de la sélection des éléments sans modifier l'état des cases à cocher
        private void checkBoxListToDo_MouseDown(object sender, MouseEventArgs e)
        {
            int index = checkBoxListToDo.IndexFromPoint(e.Location);

            if (index != ListBox.NoMatches)
            {
                Rectangle itemBounds = checkBoxListToDo.GetItemRectangle(index);
                Rectangle checkboxBounds = new Rectangle(itemBounds.X, itemBounds.Y, 16, itemBounds.Height);

                // Si le clic n'est pas sur la case à cocher, seulement sélectionner l'élément
                if (!checkboxBounds.Contains(e.Location))
                {
                    checkBoxListToDo.SelectedIndex = index;
                    e = null;  // Empêcher la modification de l'état de la case à cocher
                }
            }
        }

        // Méthode pour éditer une tâche
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
                    this.notifyIcon1.BalloonTipText = "Tâche modifiée.";
                    this.notifyIcon1.ShowBalloonTip(2000);
                }
                else
                {
                    MessageBox.Show("Veuillez entrer un texte valide pour modifier la tâche.", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            else
            {
                MessageBox.Show("Veuillez sélectionner une tâche à modifier.", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        // Méthode pour supprimer une tâche
        private void btnDeleteItem_Click(object sender, EventArgs e)
        {
            if (checkBoxListToDo.SelectedItem != null)
            {
                ToDoItem selectedTask = (ToDoItem)checkBoxListToDo.SelectedItem;

                var confirmResult = MessageBox.Show("Êtes-vous sûr de vouloir supprimer cette tâche ?",
                                                     "Confirmer la suppression",
                                                     MessageBoxButtons.YesNo,
                                                     MessageBoxIcon.Warning);
                if (confirmResult == DialogResult.Yes)
                {
                    Tasks.Remove(selectedTask);
                    dbManager.DeleteToDoItem(selectedTask);
                    this.FillLists();
                    this.notifyIcon1.BalloonTipText = "Tâche supprimée.";
                    this.notifyIcon1.ShowBalloonTip(2000);
                }
            }
            else
            {
                MessageBox.Show("Veuillez sélectionner une tâche à supprimer.", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }
        private void btnCopy_Click(object sender, EventArgs e)
        {
            // Vérifier si un texte est sélectionné dans la zone de texte
            if (!string.IsNullOrEmpty(txtNewTask.SelectedText))
            {
                // Copier le texte sélectionné dans le presse-papier
                Clipboard.SetText(txtNewTask.SelectedText);
            }
            else
            {
                // Si aucun texte n'est sélectionné, copier tout le texte
                Clipboard.SetText(txtNewTask.Text);
            }

            // Donner le focus à la zone de texte
            txtNewTask.Focus();
        }
        private void btnCut_Click(object sender, EventArgs e)
        {
            // Vérifier si un texte est sélectionné dans la zone de texte
            if (!string.IsNullOrEmpty(txtNewTask.SelectedText))
            {
                // Copier le texte sélectionné dans le presse-papier
                Clipboard.SetText(txtNewTask.SelectedText);

                // Supprimer le texte sélectionné
                txtNewTask.Text = txtNewTask.Text.Remove(txtNewTask.SelectionStart, txtNewTask.SelectionLength);
            }
            else
            {
                // Si aucun texte n'est sélectionné, couper tout le texte
                Clipboard.SetText(txtNewTask.Text);
                txtNewTask.Text = string.Empty;
            }

            // Donner le focus à la zone de texte
            txtNewTask.Focus();
        }
        private void btnPaste_Click(object sender, EventArgs e)
        {
            // Vérifier si le presse-papier contient du texte
            if (Clipboard.ContainsText())
            {
                // Coller le texte à l'emplacement du curseur
                int selectionStart = txtNewTask.SelectionStart;
                txtNewTask.Text = txtNewTask.Text.Insert(selectionStart, Clipboard.GetText());

                // Ajuster le curseur à la fin du texte collé
                txtNewTask.SelectionStart = selectionStart + Clipboard.GetText().Length;
            }

            // Donner le focus à la zone de texte
            txtNewTask.Focus();
        }
        private void menuExit_Click(object sender, EventArgs e)
        {
            // Afficher un message de confirmation avant de quitter l'application
            var confirmResult = MessageBox.Show("Êtes-vous sûr de vouloir quitter ?", "Quitter", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            // Si l'utilisateur confirme, fermer l'application
            if (confirmResult == DialogResult.Yes)
            {
                Application.Exit();
            }
        }
        private void menuNewItem_Click(object sender, EventArgs e)
        {
            // Simuler un clic sur le bouton d'ajout de tâche
            btnAddItem_Click(sender, e);
        }
        private void menuDeleteItem_Click(object sender, EventArgs e)
        {
            // Simuler un clic sur le bouton de suppression de tâche
            btnDeleteItem_Click(sender, e);
        }
        private void menuEditItem_Click(object sender, EventArgs e)
        {
            // Simuler un clic sur le bouton de modification de tâche
            btnEditItem_Click(sender, e);
        }

    }
}
