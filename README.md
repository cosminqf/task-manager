# Task Manager

O aplicație web complexă pentru gestionarea proiectelor, echipelor și sarcinilor, dezvoltată utilizând ASP.NET Core 9.0 MVC. Platforma facilitează colaborarea în echipă, urmărirea progresului prin dashboard-uri personalizate și administrarea eficientă a resurselor.

## Descriere

Proiectul propune o soluție centralizată pentru managementul activităților. Aplicația este împărțită în două componente majore: partea organizatorică (gestiune utilizatori, proiecte, administrare) și partea operativă (task-uri, fișiere, dashboard). Sistemul include funcționalități avansate precum integrarea AI pentru generarea de rezumate și suport pentru fișiere media.

## Funcționalități Principale

### Autentificare și Utilizatori
* Sistem complet de autentificare folosind ASP.NET Identity.
* Roluri definite: Administrator și User (organizatorul este un rol logic la nivel de proiect).
* Pagini pentru Login, Înregistrare și Logout.

### Gestiune Proiecte și Echipe
* CRUD complet pentru proiecte (Titlu, Descriere, Dată, Creator).
* Posibilitatea de a adăuga și șterge membri din proiecte.
* Vizualizarea listei de membri asociați fiecărui proiect.

### Gestiune Task-uri
* Creare, editare și ștergere task-uri.
* Atribute task: Titlu, Descriere, Status (Not Started, In Progress, Completed), Date (Start/End).
* Upload fișiere media (imagini) și embed video (link YouTube).
* Asignarea task-urilor către membrii echipei.
* Validări pentru datele calendaristice.

### Colaborare și Social
* Sistem de comentarii pentru fiecare task.
* Afișare cronologică a discuțiilor.

### Dashboard și Statistici
* Dashboard personalizat pentru fiecare utilizator.
* Statistici vizuale (număr task-uri în lucru, urgente).
* Filtrare task-uri după status sau urgență (următoarele 24h).

### Administrare
* Panel dedicat administratorilor.
* Gestiune globală a utilizatorilor (ban/delete).
* Gestiune globală a proiectelor (ștergere forțată).

### Integrare AI
* Generare automată de rapoarte și rezumate de proiect folosind API extern (OpenAI/Azure).

## Tehnologii Utilizate

* **Backend:** ASP.NET Core 9.0 MVC, Entity Framework Core
* **Bază de date:** MySQL
* **Frontend:** Razor Views, Bootstrap
* **Containerizare:** Docker
* **Altele:** ASP.NET Identity, servicii externe AI

## Instalare și Configurare

1. Clonează repository-ul:
   git clone https://github.com/cosminqf/task-manager.git

2. Configurează baza de date:
   Asigură-te că ai un server MySQL activ sau un container Docker configurat.
   Actualizează connection string-ul în appsettings.json.

3. Aplică migrațiile:
   dotnet ef database update

4. Rulează aplicația:
   dotnet run

## Structura Bazei de Date

Entitățile principale ale sistemului sunt:
* ApplicationUser (extinde IdentityUser)
* Project
* Task
* Comment
* ProjectMembers (tabelă de legătură)

## Autori

* Danciu Cosmin-Alexandru
* Popescu Ilioniu Andrei
