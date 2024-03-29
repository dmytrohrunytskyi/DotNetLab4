﻿using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Windows.Forms;

namespace ChatLibrary
{
    [ServiceContract(CallbackContract = typeof(IChatServiceCallback))]
    public interface IChatService
    {
        [OperationContract]
        int Connect(string username);
        [OperationContract(IsOneWay = true)]
        void Disconnect(int id);
        [OperationContract(IsOneWay = true)]
        void SendMessage(string message, string nickname);
        [OperationContract(IsOneWay = true)]
        void UserJoin(string nickname);

    }

    [ServiceContract]
    public interface IChatServiceCallback
    {
        [OperationContract(IsOneWay = true)]
        void SendMessageToClient(string message, string nickname);
        [OperationContract(IsOneWay = true)]
        void UserJoinToServer(string nickname);
    }

    public class ChatUser
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public OperationContext Context { get; set; }
    }

    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single)]
    public class ChatService : IChatService
    {
        List<ChatUser> usersList = new List<ChatUser>();
        int nextUserId = 1;
        public int Connect(string username)
        {
            ChatUser user = new ChatUser()
            {
                Id = nextUserId++,
                Name = username,
                Context = OperationContext.Current,
            };

            //foreach (ChatUser u in usersList)
            //{
            //    if (u.Name == username)
            //        MessageBox.Show("Користувач з таким ім'ям вже існує.", "Помилка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            //    else
            //        usersList.Add(user);
            //}    

            usersList.Add(user);
            return user.Id;
        }

        public void Disconnect(int id)
        {
            var user = usersList.FirstOrDefault(x => x.Id == id);
            if (user != null)
                usersList.Remove(user);
        }

        public void SendMessage(string message, string nickname)
        {
            foreach (ChatUser user in usersList)
            {
                user.Context.GetCallbackChannel<IChatServiceCallback>().SendMessageToClient(message, nickname);
            }
        }

        public void UserJoin(string nickname)
        {
            foreach (ChatUser user in usersList)
            {
                    user.Context.GetCallbackChannel<IChatServiceCallback>().UserJoinToServer(nickname);
            }
        }
    }
}