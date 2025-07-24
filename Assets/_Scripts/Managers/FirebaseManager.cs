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

    public event Action<string> OnAuthFailed; // Erro
    public event Action OnAuthSuccess;        // Login ou registro com sucesso
    public event Action OnLogout;             // Logout efetuado

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
            var status = task.Result;
            if (status == DependencyStatus.Available)
            {
                Auth = FirebaseAuth.DefaultInstance;
                User = Auth.CurrentUser;
                Debug.Log("✅ Firebase inicializado.");
            }
            else
            {
                Debug.LogError($"❌ Firebase erro: {status}");
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
                Debug.LogError($"❌ Erro ao registrar: {erro}");
                OnAuthFailed?.Invoke(erro);
            }
            else
            {
                Debug.Log("✅ Usuário registrado com sucesso.");
                // Nao invoca OnAuthSuccess aqui, pois nao queremos logar automaticamente
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
                User = task.Result.User;
                Debug.Log("Usuário logado com sucesso.");
                OnAuthSuccess?.Invoke(); // Invoca OnAuthSuccess apos login bem-sucedido
            }
        });
    }

    public void Logout()
    {
        if (Auth != null && User != null)
        {
            Auth.SignOut();
            User = null;
            Debug.Log("✅ Usuário deslogado.");
            OnLogout?.Invoke();
        }
        else
        {
            Debug.LogWarning("⚠️ Logout chamado sem usuário logado.");
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
                Debug.LogError($"❌ Erro ao enviar email de reset: {erro}");
                OnAuthFailed?.Invoke(erro);
            }
            else
            {
                Debug.Log("✅ Email de redefinição enviado.");
                OnAuthSuccess?.Invoke(); // Invoca OnAuthSuccess para feedback positivo
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
            // Remove as linhas que causam o erro de compilação
            // case AuthError.AccountExistsWithDifferentCredential:
            // return "Já existe uma conta com esse e-mail usando outro método de login.";
            // ... e qualquer outra que cause o mesmo erro.

            // Você pode deixar as que funcionam, ou simplesmente usar o default:
            switch ((AuthError)firebaseEx.ErrorCode)
            {
                // Mantenha apenas as que sua versão do Firebase SDK suporta, ou...
                // ... remova todas e use apenas o default para uma abordagem mais simples:
                // case AuthError.WeakPassword:
                //     return "A senha é muito fraca. Mínimo de 6 caracteres.";
                // case AuthError.InvalidEmail:
                //     return "Formato de e-mail inválido.";
                // case AuthError.MissingEmail:
                //     return "E-mail ausente.";
                // case AuthError.NetworkRequestFailed:
                //     return "Falha de rede. Verifique sua conexão com a internet.";
                // case AuthError.TooManyRequests:
                //     return "Muitas tentativas. Tente novamente mais tarde.";
                // case AuthError.OperationNotAllowed:
                //     return "Operação não permitida. Verifique suas configurações de autenticação no Firebase.";
                // case AuthError.UserDisabled:
                //     return "Sua conta foi desativada.";
                default:
                    // Mensagem genérica para outros erros, incluindo aqueles que não são reconhecidos
                    return "Ocorreu um erro: " + firebaseEx.Message; // Use a mensagem original do Firebase
            }
        }
    }
    return "Erro desconhecido."; // Caso não seja uma FirebaseException ou não tenha InnerExceptions
}
}