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

    // Eventos para comunicação com outros scripts
    public event Action<string> OnAuthFailed;    // Disparado quando ocorre erro de autenticação
    public event Action OnAuthSuccess;           // Disparado no login bem sucedido
    public event Action OnLogout;                // Disparado no logout

    // No FirebaseManager.cs

private void Awake()
{
    if (Instance == null)
    {
        Instance = this;
        // Remova ou comente a linha abaixo se você quer que o FirebaseManager seja destruído ao carregar uma nova cena
        DontDestroyOnLoad(gameObject); 
        Debug.Log("FirebaseManager: Instância criada.");
        InitializeFirebase();
    }
    else
    {
        Debug.LogWarning("FirebaseManager: Instância já existe. Destruindo esta nova.");
        Destroy(gameObject);
    }
}

    // No FirebaseManager.cs
private void InitializeFirebase()
{
    Debug.Log("FirebaseManager: Iniciando checagem de dependências do Firebase...");
    FirebaseApp.CheckAndFixDependenciesAsync().ContinueWith(task =>
    {
        var status = task.Result;
        if (status == DependencyStatus.Available)
        {
            Auth = FirebaseAuth.DefaultInstance;
            Auth.StateChanged += AuthStateChanged; // Inscreve-se no evento
            User = Auth.CurrentUser; // Obtém o usuário na inicialização
            Debug.Log("✅ FirebaseManager: Firebase inicializado com sucesso.");

            // **Chame AuthStateChanged uma vez na inicialização**
            // para que a lógica de "já logado" seja processada e o OnAuthSuccess seja disparado.
            AuthStateChanged(this, EventArgs.Empty); 
        }
        else
        {
            Debug.LogError($"❌ FirebaseManager: Erro ao inicializar Firebase: {status}");
            OnAuthFailed?.Invoke("Falha ao inicializar o serviço de autenticação.");
        }
    });
}

    // No FirebaseManager.cs

private void AuthStateChanged(object sender, System.EventArgs eventArgs)
{
    FirebaseUser newUser = Auth.CurrentUser;

    // Se o novo usuário é diferente do usuário anterior (ou um era null e o outro não)
    if (newUser != User) 
    {
        User = newUser; // Atualiza a referência do usuário
        if (User != null && User.IsValid())
        {
            Debug.Log($"✅ FirebaseManager: Estado de autenticação alterado - Usuário logado: {User.Email}. Disparando OnAuthSuccess.");
            OnAuthSuccess?.Invoke(); // Dispara o evento de sucesso para a UI
        }
        else
        {
            Debug.Log("⚠️ FirebaseManager: Estado de autenticação alterado - Usuário deslogado ou nulo. Disparando OnLogout.");
            OnLogout?.Invoke(); // Dispara o evento de logout para a UI
        }
    }
    // ESTE ELSE IF É FUNDAMENTAL para quando o jogo inicia e o usuário JÁ ESTÁ logado.
    // O AuthStateChanged é chamado, mas newUser e User são ambos o mesmo (o usuário logado),
    // então a primeira condição (newUser != User) seria falsa.
    else if (User != null && User.IsValid()) 
    {
        Debug.Log($"FirebaseManager: Usuário já estava logado ({User.Email}). Garantindo que a UI responda via OnAuthSuccess.");
        OnAuthSuccess?.Invoke(); // Garante que a UI reaja ao estado logado na inicialização
    }
    else // Nenhuma mudança e ainda não logado
    {
        Debug.Log("FirebaseManager: Nenhuma mudança no estado de autenticação. Ninguém logado.");
        OnLogout?.Invoke(); // Garante que a UI mostre o menu principal ou painel de login.
    }
}

    private void OnDestroy()
    {
        // Desinscreva o evento para evitar memory leaks
        if (Auth != null)
        {
            Auth.StateChanged -= AuthStateChanged;
        }
    }

    public void RegisterUser(string email, string password)
    {
        Debug.Log($"FirebaseManager: Tentando registrar usuário com email: {email}");
        if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
        {
            OnAuthFailed?.Invoke("Email e senha não podem ser vazios para o registro.");
            Debug.LogWarning("FirebaseManager: Tentativa de registro com email ou senha vazios.");
            return;
        }
        if (Auth == null)
        {
            OnAuthFailed?.Invoke("Serviço de autenticação não inicializado.");
            Debug.LogError("FirebaseManager: Auth é nulo ao tentar registrar.");
            return;
        }

        Auth.CreateUserWithEmailAndPasswordAsync(email, password).ContinueWith(task =>
        {
            if (task.IsFaulted || task.IsCanceled)
            {
                string erro = GetFirebaseError(task.Exception);
                Debug.LogError($"❌ FirebaseManager: Erro ao registrar: {erro}");
                OnAuthFailed?.Invoke(erro);
            }
            else
            {
                Debug.Log("✅ FirebaseManager: Usuário registrado com sucesso. User: " + task.Result.User.Email);
                // Não loga automaticamente, a UI (SceneFlowManager) deve guiar para a tela de login.
                // O SceneFlowManager pode exibir um feedback e então mudar para a tela de login.
            }
        });
    }

    public void LoginUser(string email, string password)
    {
        Debug.Log($"FirebaseManager: Tentando logar usuário com email: {email}");
        if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
        {
            OnAuthFailed?.Invoke("Email e senha não podem ser vazios para o login.");
            Debug.LogWarning("FirebaseManager: Tentativa de login com email ou senha vazios.");
            return;
        }
        if (Auth == null)
        {
            OnAuthFailed?.Invoke("Serviço de autenticação não inicializado.");
            Debug.LogError("FirebaseManager: Auth é nulo ao tentar logar.");
            return;
        }

        Auth.SignInWithEmailAndPasswordAsync(email, password).ContinueWith(task =>
        {
            if (task.IsFaulted || task.IsCanceled)
            {
                string erro = GetFirebaseError(task.Exception);
                Debug.LogError($"❌ FirebaseManager: Erro no login: {erro}");
                OnAuthFailed?.Invoke(erro);
            }
            else
            {
                User = task.Result.User;
                Debug.Log($"✅ FirebaseManager: Usuário logado com sucesso. User: {User.Email}");
                OnAuthSuccess?.Invoke(); // Dispara evento de sucesso (Será capturado por AuthStateChanged também)
            }
        });
    }

    public void Logout()
    {
        Debug.Log("FirebaseManager: Tentando fazer logout.");
        if (Auth != null && User != null)
        {
            Auth.SignOut();
            User = null; // Limpa o usuário atual
            Debug.Log("✅ FirebaseManager: Usuário deslogado com sucesso.");
            OnLogout?.Invoke(); // Dispara evento de logout (Será capturado por AuthStateChanged também)
        }
        else
        {
            Debug.LogWarning("⚠️ FirebaseManager: Logout chamado sem usuário logado ou Auth não inicializado.");
            OnLogout?.Invoke(); // Ainda pode disparar para que a UI responda, mas sem feedback de erro.
        }
    }

    public void SendPasswordResetEmail(string email)
    {
        Debug.Log($"FirebaseManager: Tentando enviar email de redefinição para: {email}");
        if (string.IsNullOrEmpty(email))
        {
            OnAuthFailed?.Invoke("Email não pode ser vazio para redefinir senha.");
            Debug.LogWarning("FirebaseManager: Tentativa de enviar email de reset com email vazio.");
            return;
        }
        if (Auth == null)
        {
            OnAuthFailed?.Invoke("Serviço de autenticação não inicializado.");
            OnAuthFailed?.Invoke("Serviço de autenticação não inicializado.");
            Debug.LogError("FirebaseManager: Auth é nulo ao tentar enviar email de reset.");
            return;
        }

        Auth.SendPasswordResetEmailAsync(email).ContinueWith(task =>
        {
            if (task.IsFaulted || task.IsCanceled)
            {
                string erro = GetFirebaseError(task.Exception);
                Debug.LogError($"❌ FirebaseManager: Erro ao enviar email de reset: {erro}");
                OnAuthFailed?.Invoke(erro);
            }
            else
            {
                Debug.Log("✅ FirebaseManager: Email de redefinição enviado com sucesso.");
                OnAuthFailed?.Invoke("Email de redefinição de senha enviado. Verifique sua caixa de entrada."); // Usando OnAuthFailed para feedback para o usuário
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
                AuthError errorCode = (AuthError)firebaseEx.ErrorCode;
                Debug.LogError($"Firebase Error Code: {errorCode}, Message: {firebaseEx.Message}");
                switch (errorCode)
                {
                    case AuthError.MissingEmail:
                        return "Por favor, digite seu email.";
                    case AuthError.InvalidEmail:
                        return "Email inválido.";
                    case AuthError.WrongPassword:
                        return "Senha incorreta.";
                    case AuthError.UserNotFound:
                        return "Usuário não encontrado. Crie uma conta.";
                    case AuthError.WeakPassword:
                        return "Senha muito fraca. Mínimo 6 caracteres.";
                    case AuthError.EmailAlreadyInUse:
                        return "Este email já está em uso.";
                    case AuthError.NetworkRequestFailed:
                        return "Erro de conexão. Verifique sua internet.";
                    case AuthError.UserDisabled:
                        return "Sua conta foi desativada.";
                    case AuthError.TooManyRequests:
                        return "Muitas tentativas. Tente novamente mais tarde.";
                    case AuthError.RequiresRecentLogin:
                        return "Esta operação requer autenticação recente. Faça login novamente.";
                    default:
                        return "Erro de autenticação: " + firebaseEx.Message;
                }
            }
        }
        return "Erro desconhecido.";
    }
}