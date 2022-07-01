using MXRClasses;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Storyboard
{
    #region Interactions

    //Base Class for any interaction
    [System.Serializable]
    public class Interaction
    {
        public InteractionType type;
        public string title;
        public string description;
        public string trigger;
        public bool scored;
        public Feedback feedback;
        public string optionalDetails;

        public Interaction(InteractionType type = InteractionType.Basic,
            string title = "", string description = "", bool scored = false, 
            string optionalDetails = "", string trigger = "")
        {
            this.type = type;
            this.title = title;
            this.description = description;
            this.trigger = trigger;
            this.scored = scored;
            this.optionalDetails = optionalDetails;
            this.feedback = new Feedback();
            this.feedback.correctMessage = "";
            this.feedback.incorrectMessage = "";
            this.feedback.partialcorrectMessage = "";
        }

        //Copies all data from another interaction except type
        public virtual void Copy(Interaction interaction)
        {
            //this.type = interaction.type;
            this.title = interaction.title;
            this.description = interaction.description;
            this.trigger = interaction.trigger;
            this.scored = interaction.scored;
            this.optionalDetails = interaction.optionalDetails;
            this.feedback = interaction.feedback;
        }
    }
    
    //Extends Interaction to contain answers and feedback
    [System.Serializable]
    public class Question : Interaction
    {
        //Question
        public string question;
        public QuestionDialogueType qType;
        public List<string> answers = new List<string>();
        public List<bool> correctAnswers = new List<bool>();
        //Feedback
        public string correctMessage;
        public string incorrectMessage;
        public string partialcorrectMessage;

        public Question(string question = "", QuestionDialogueType type = QuestionDialogueType.MultiChoicePanel, 
            string title = "", string description = "", bool scored = false, string optionalDetails = "",
            string trigger = "") 
            : base(type:InteractionType.Question, title, description, scored,optionalDetails, trigger)
        {
            this.question = question;
            this.qType = type;
        }

        //Copies any data from base interactions, and then any data pertaining to questions
        public override void Copy(Interaction interaction)
        {
            base.Copy(interaction);
            type = InteractionType.Question;
            
            if (interaction is Question q)
            {
                question = q.question;
                qType = q.qType;
                answers = q.answers;
                correctAnswers = q.correctAnswers;
                correctMessage = q.correctMessage;
                incorrectMessage = q.incorrectMessage;
                partialcorrectMessage = q.partialcorrectMessage;

            }
            //message from Messages turn into question
            if (interaction is Message m)
            {
                question = m.message;
            }
        }
    }

    //Extends Interaction to include a message
    [System.Serializable]
    public class Message : Interaction
    {
        public string message;

        public Message(string message = "", string title = "", string description = "",
            bool scored = false, string optionalDetails = "",
            string trigger = "") 
            : base(type: InteractionType.Message, title, description, scored, optionalDetails, trigger)
        {
            this.message = message;
        }

        //Copies any data from base interactions, and then any data pertaining to messages
        public override void Copy(Interaction interaction)
        {
            base.Copy(interaction);
            type = InteractionType.Message;
            
            if (interaction is Message m)
            {
                message = m.message;
            }
            if (interaction is Question q)
            {
                message = q.question;
            }
        }
    }

    //Feedback data for any scored task
    [System.Serializable]
    public class Feedback
    {
        public string correctMessage = "";
        public string incorrectMessage = "";
        public string partialcorrectMessage = "";
    }

    //Serializable form of TaskList
    [System.Serializable]
    public class InteractionList
    {
        public string title;
        public int orderType;
        [SerializeReference] public List<Interaction> interactions;
    }

    //Serializable form of Storyboard
    [System.Serializable]
    public class Module
    {
        public string title;
        public List<List<InteractionList>> scenarios;
    }

    #endregion

    [System.Serializable]
    public enum InteractionType
    {
        Basic,
        Area,
        Touch,
        Look,
        Question,
        Message
    }
}
