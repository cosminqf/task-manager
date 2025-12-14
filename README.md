# task-manager

Raport proiect: https://1drv.ms/w/c/ec17cb32d958e7ed/IQD7JlvuI5tzSqoNSPv3qD80AVd1UzOkv_TOvM_o8b0_N18?e=ihc8Yf

Board Trello: https://trello.com/invite/b/69133955d10c9b794cc4851e/ATTI0e9c64b8393e10c849a8971575f84508192C5ED2/taskmanager

PLANIFICARE PROIECT: PLATFORMA TASK MANAGEMENT (ASP.NET Core MVC)

INSTRUCȚIUNI GENERALE:
- Fiecare membru implementează funcționalitățile cap-coadă (Database -> Model -> Controller -> View).
- Fiecare membru este responsabil de seed-ul datelor pentru entitățile sale.
- Codul se va uni folosind un repository comun (GitHub/GitLab).

--------------------------------------------------------------------------------
0. RESPONSABILITĂȚI COMUNE (Start & Final)
--------------------------------------------------------------------------------
- Setup inițial: Creare soluție, instalare Entity Framework Core, Configurare Connection String.
- Layout General: Creare `_Layout.cshtml` (Navbar, Footer, importuri CSS/JS).
- Req 11: 
    - Seed Data: Fiecare își scrie seed-ul pentru partea sa.
    - README: Fiecare documentează modulele implementate.

COSMIN: INFRASTRUCTURĂ, ADMINISTRARE & AI
(Focus: Useri, Proiecte, Echipe, Rapoarte)

1. Autentificare & Roluri (Req 1)
   [Backend] Configurare ASP.NET Identity.
   [Backend] Seed pentru rolurile: Admin, User (Organizatorul este logică, nu rol DB).
   [Frontend] Formulare de Login, Register, Logout.

2. Pagina de Prezentare (Req 2)
   [Frontend] Landing page (HomeController) cu descriere, beneficii, design atractiv.
   [Logic] Dacă userul e deja logat, redirect către Dashboard sau lista proiecte.

3. Gestionarea Proiectelor (Req 3)
   [Entitate] `Project` (Id, Title, Description, DateCreated, CreatorId).
   [Backend] CRUD complet pentru Proiecte.
   [Logic] La crearea unui proiect, userul curent devine automat "Organizator".

4. Gestionarea Echipei (Req 4)
   [Entitate] Tabela de legătură `ProjectMembers` (ProjectId, UserId).
   [Backend] Logică de a adăuga/șterge un user dintr-un proiect.
   [Frontend] Interfață în pagina proiectului pentru a invita membri (select user dintr-o listă).
   [Frontend] Lista membrilor afișată în pagina proiectului.

5. Componenta AI - Rezumat Proiect (Req 9)
   [Backend] Integrare API (OpenAI/Azure) care primește datele proiectului (titlu + lista task-uri).
   [Frontend] Buton "Generează Raport" în pagina proiectului.
   [Frontend] Afișarea răspunsului generat (rezumat progres, deadline-uri).

6. Administrare Platformă (Req 10)
   [Controller] `AdminController` accesibil doar cu rolul Administrator.
   [Frontend] Tabel cu toți userii (ban/delete).
   [Frontend] Tabel cu toate proiectele (delete forțat dacă e conținut inadecvat).

ANDREI: WORKFLOW, TASK-URI & DASHBOARD
(Focus: Task-uri, Interacțiune, Media, User Experience)

1. Gestionarea Task-urilor (Req 5)
   [Entitate] `Task` (Id, Title, Description, Status, StartDate, EndDate, MediaUrl, ProjectId).
   [Backend] CRUD complet pentru Task-uri.
   [Backend] Implementare Upload poze (IFormFile) și logică embed video (Youtube Link).
   [Frontend] Formular adăugare task (cu DatePicker și File Input). Validări (End > Start).

2. Atribuire și Status (Req 6)
   [Backend] Logică de asignare: Un task este legat de un `UserId` (Membru).
   [Backend] Logică Status: Not Started -> In Progress -> Completed.
   [Frontend] Dropdown în formularul de task pentru a alege membrul (din lista membrilor proiectului).
   [Frontend] Butoane rapide de schimbare status în vizualizarea task-ului.

3. Sistem de Comentarii (Req 7)
   [Entitate] `Comment` (Id, Content, DateAdded, TaskId, UserId).
   [Backend] CRUD Comentarii (Adăugare, Editare, Ștergere - doar propriile comentarii).
   [Frontend] Secțiune de comentarii sub fiecare Task (afișare cronologică).

4. Dashboard Personalizat (Req 8)
   [Controller] `DashboardController`.
   [Backend] Query-uri complexe:
       - Selectează toate task-urile unde AssignedUserId == CurrentUser.
       - Grupează task-urile după status sau deadline.
       - Identifică task-urile cu deadline depășit sau urgent (următoarele 24h).
   [Frontend] Pagina "My Dashboard":
       - Carduri cu statistici (ex: "3 Task-uri în lucru").
       - Listă clară cu task-urile mele, colorate în funcție de urgență.
       - Filtre (Butoane: "Arată doar Finalizate", "Arată Urgente").

SUGESTIE ORDINE DE LUCRU (SINCRONIZARE)
Săptămâna 1:
- COSMIN: Setup Identity + Proiecte (CRUD).
- ANDREI: Setup Task-uri (CRUD simplu, fără asignare încă).

Săptămâna 2:
- COSMIN: Implementează Echipe (adaugă useri în proiecte).
- ANDREI: Leagă Task-urile de Useri (Assign) și face Upload Media.

Săptămâna 3:
- COSMIN: Implementează AI Summary (acum are date reale din task-uri).
- ANDREI: Implementează Comentarii și Dashboard-ul personal.

Săptămâna 4:
- COSMIN: Admin Panel.
- ANDREI: Finalizare UI/UX Dashboard.
- Ambii: Seed data final, verificări, README.
