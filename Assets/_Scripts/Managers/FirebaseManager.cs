using System;
using System.Threading.Tasks;
using UnityEngine;
using Firebase;
using Firebase.Auth;

public class FirebaseManager : MonoBehaviour
{
    public static FirebaseManager Instance { get; private set; }

    public FirebaseAuth Auth { get; private set; }
    public FirebaseUser User { get; private set; }

    public event Action<string> OnAuthFailed;    // mensagem de erro
    public event Action OnAuthSuccess;           // login ou registro ok
    public event Action OnLogout;                 // logout efetuado

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeFirebase();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void InitializeFirebase()
    {
        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWith(task =>
        {
            var dependencyStatus = task.Result;
            if (dependencyStatus == DependencyStatus.Available)
            {
                Auth = FirebaseAuth.DefaultInstance;
                User = Auth.CurrentUser;
                Debug.Log("Firebase Auth inicializado.");
            }
            else
            {
                Debug.LogError($"Erro Firebase: {dependencyStatus}");
                OnAuthFailed?.Invoke("Falha ao inicializar Firebase.");
            }
        });
    }

    public void RegisterUser(string email, string password)
    {
        if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
        {
            OnAuthFailed?.Invoke("Email e senha não podem ser vazios.");
            return;
        }

        Auth.CreateUserWithEmailAndPasswordAsync(email, password).ContinueWith(task =>
        {
            if (task.IsFaulted || task.IsCanceled)
            {
                string erro = GetFirebaseError(task.Exception);
                Debug.LogError($"Erro Register: {erro}");
                OnAuthFailed?.Invoke(erro);
            }
            else
            {
                User = task.Result.User;  // CORREÇÃO AQUI
                Debug.Log("Usuário registrado com sucesso.");
                OnAuthSuccess?.Invoke();
            }
        });
    }

    public void LoginUser(string email, string password)
    {
        if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
        {
            OnAuthFailed?.Invoke("Email e senha não podem ser vazios.");
            return;
        }

        Auth.SignInWithEmailAndPasswordAsync(email, password).ContinueWith(task =>
        {
            if (task.IsFaulted || task.IsCanceled)
            {
                string erro = GetFirebaseError(task.Exception);
                Debug.LogError($"Erro Login: {erro}");
                OnAuthFailed?.Invoke(erro);
            }
            else
            {
                User = task.Result.User;  // CORREÇÃO AQUI
                Debug.Log("Usuário logado com sucesso.");
                OnAuthSuccess?.Invoke();
            }
        });
    }

    public void Logout()
    {
        if (Auth != null && User != null)
        {
            Auth.SignOut();
            User = null;
            Debug.Log("Usuário deslogado.");
            OnLogout?.Invoke();
        }
        else
        {
            Debug.LogWarning("Logout chamado sem usuário logado.");
        }
    }

    public void SendPasswordResetEmail(string email)
    {
        if (string.IsNullOrEmpty(email))
        {
            OnAuthFailed?.Invoke("Email não pode ser vazio.");
            return;
        }

        Auth.SendPasswordResetEmailAsync(email).ContinueWith(task =>
        {
            if (task.IsFaulted || task.IsCanceled)
            {
                string erro = GetFirebaseError(task.Exception);
                Debug.LogError($"Erro reset senha: {erro}");
                OnAuthFailed?.Invoke(erro);
            }
            else
            {
                Debug.Log("Email de reset enviado com sucesso.");
                OnAuthSuccess?.Invoke();
            }
        });
    }

    private string GetFirebaseError(AggregateException exception)
    {
        if (exception == null) return "Erro desconhecido.";

        foreach (var e in exception.Flatten().InnerExceptions)
        {
            if (e is FirebaseException firebaseEx)
            {
                return firebaseEx.Message;
            }
        }
        return "Erro desconhecido.";
    }
}
