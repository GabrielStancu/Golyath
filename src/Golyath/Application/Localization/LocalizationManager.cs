using CommunityToolkit.Mvvm.ComponentModel;
using Golyath.Core.Enums;
using System.Globalization;

namespace Golyath.Application.Localization;

/// <summary>
/// Dictionary-based localization singleton. XAML pages bind to it using
/// <c>Text="{Binding [Key], Source={x:Static loc:LocalizationManager.Instance}}"</c>.
/// When <see cref="SetLanguage"/> is called it raises PropertyChanged("Item[]"),
/// which causes every indexer binding on the page to re-evaluate immediately.
/// </summary>
public sealed partial class LocalizationManager : ObservableObject
{
    public static readonly LocalizationManager Instance = new();

    private AppLanguage _current = AppLanguage.English;

    private LocalizationManager() { }

    // ── Indexer (used by all XAML bindings) ────────────────────────────────
    public string this[string key]
    {
        get
        {
            if (_current == AppLanguage.Romanian && _ro.TryGetValue(key, out var ro))
                return ro;
            return _en.TryGetValue(key, out var en) ? en : $"[{key}]";
        }
    }

    // ── Apply language ─────────────────────────────────────────────────────
    public void SetLanguage(AppLanguage language)
    {
        _current = language;

        var culture = language == AppLanguage.Romanian
            ? CultureInfo.GetCultureInfo("ro-RO")
            : CultureInfo.GetCultureInfo("en-US");

        Thread.CurrentThread.CurrentCulture   = culture;
        Thread.CurrentThread.CurrentUICulture = culture;
        CultureInfo.DefaultThreadCurrentCulture   = culture;
        CultureInfo.DefaultThreadCurrentUICulture = culture;

        // Notify every indexer binding across all live pages
        var dispatch = Microsoft.Maui.Controls.Application.Current?.Dispatcher;
        if (dispatch is not null && dispatch.IsDispatchRequired)
            dispatch.Dispatch(() => OnPropertyChanged("Item[]"));
        else
            OnPropertyChanged("Item[]");
    }

    // ── English strings ────────────────────────────────────────────────────
    private static readonly Dictionary<string, string> _en = new()
    {
        // Welcome
        ["Welcome_Subtitle"]            = "Your Personal Training Intelligence",
        ["Welcome_StartTraining"]       = "START TRAINING",
        ["Welcome_RestoreBackup"]       = "Restore from Backup",

        // Profile Setup (onboarding step 1)
        ["ProfileSetup_Title"]          = "Tell us about you",
        ["ProfileSetup_Subtitle"]       = "Personalise your training experience",
        ["Field_Nickname"]              = "NICKNAME",
        ["Field_NicknamePlaceholder"]   = "e.g. Alex",
        ["Field_Birthday"]              = "BIRTHDAY",
        ["Field_Units"]                 = "UNITS",
        ["Field_Gender"]                = "GENDER",
        ["Field_Language"]              = "LANGUAGE",
        ["ProfileSetup_Continue"]       = "Continue",

        // Goal Setup (onboarding step 2)
        ["GoalSetup_Title"]             = "What's your goal?",
        ["GoalSetup_Subtitle"]          = "We'll tailor your workouts and suggestions",
        ["Goal_Strength"]               = "Strength",
        ["Goal_Strength_Desc"]          = "Build raw power and lift heavier",
        ["Goal_Hypertrophy"]            = "Hypertrophy",
        ["Goal_Hypertrophy_Desc"]       = "Maximise muscle size and definition",
        ["Goal_FatLoss"]                = "Fat Loss",
        ["Goal_FatLoss_Desc"]           = "Burn fat while preserving muscle",
        ["Goal_Balanced"]               = "Balanced",
        ["Goal_Balanced_Desc"]          = "All-around fitness and health",
        ["GoalSetup_LetsGo"]            = "Let's Go!",

        // Onboarding Complete
        ["Complete_Title"]              = "You're all set!",
        ["Complete_Subtitle"]           = "Your profile is ready. Time to start tracking your progress and crushing your goals.",
        ["Complete_Button"]             = "Start Training →",

        // Dashboard
        ["Dashboard_TodaySession"]      = "TODAY'S SESSION",
        ["Dashboard_StartSession"]      = "START SESSION",
        ["Dashboard_Sessions"]          = "SESSIONS",
        ["Dashboard_ThisWeek"]          = "THIS WEEK",
        ["Dashboard_Streak"]            = "STREAK",
        ["Dashboard_MyRoutines"]        = "MY ROUTINES",
        ["Dashboard_SeeAll"]            = "See all ›",
        ["Dashboard_New"]               = "+ NEW",
        ["Dashboard_NoRoutines"]        = "No routines yet",
        ["Dashboard_NoRoutinesHint"]    = "Tap '+ NEW' to create your first routine.",
        ["Dashboard_GoodMorning"]       = "GOOD MORNING",
        ["Dashboard_GoodAfternoon"]     = "GOOD AFTERNOON",
        ["Dashboard_GoodEvening"]       = "GOOD EVENING",
        ["Dashboard_FreeWorkout"]       = "Free Workout",
        ["Dashboard_FreeWorkoutHint"]   = "Add exercises on the fly",

        // History
        ["History_Title"]               = "History",
        ["History_SessionsThisMonth"]   = "Sessions this month",
        ["History_TotalVolume"]         = "Total volume",
        ["History_WeeklyVolumeKg"]      = "WEEKLY VOLUME (KG)",
        ["History_4WeekTrend"]          = "4-WEEK TREND",
        ["History_RecentSessions"]      = "RECENT SESSIONS",
        ["History_NoWorkouts"]          = "No workouts found",
        ["History_NoWorkoutsHint"]      = "Complete a workout to see your history here",

        // Analytics
        ["Analytics_Title"]             = "Progress",
        ["Analytics_1RMTrend"]          = "1RM TREND",
        ["Analytics_NoData"]            = "No data for the selected exercise / period.",
        ["Analytics_MuscleBalance"]     = "MUSCLE BALANCE",
        ["Analytics_Recovery"]          = "RECOVERY",
        ["Analytics_Intensity"]         = "INTENSITY",
        ["Analytics_Insights"]          = "INSIGHTS",
        ["Analytics_NoInsights"]        = "No insights yet — keep training!",

        // Goals
        ["Goals_Title"]                 = "Goals",
        ["Goals_Subtitle"]              = "Track your strength, consistency, and balance",
        ["Goals_NoGoals"]               = "No Goals Yet",
        ["Goals_NoGoalsHint"]           = "Tap + to set your first goal and start tracking progress.",
        ["Goals_Active"]                = "ACTIVE",
        ["Goals_Completed"]             = "COMPLETED",
        ["Goals_Tab"]                   = "Goals",
        ["Goals_PRs"]                   = "PRs",
        ["Goals_PageTitle"]             = "Goals & PRs",

        // Suggestions
        ["Suggestions_Title"]           = "Smart Suggestions",
        ["Suggestions_Subtitle"]        = "Personalised recommendations based on your training data",
        ["Suggestions_Empty"]           = "No Suggestions Right Now",
        ["Suggestions_EmptyHint"]       = "Keep training! Suggestions will appear once you have enough data to analyse.",

        // Exercise Library
        ["Exercises_Title"]             = "Exercise Library",
        ["Exercises_Subtitle"]          = "Browse and manage exercises",
        ["Exercises_SearchPlaceholder"] = "Search exercises…",
        ["Exercises_NoExercises"]       = "No exercises found",
        ["Exercises_NoExercisesHint"]   = "Try adjusting your search or filters",
        ["Exercises_Custom"]            = "Custom",

        // Workout / Routines
        ["Routines_YourRoutines"]       = "YOUR ROUTINES",
        ["Routines_Train"]              = "Train",
        ["Routines_NoRoutines"]         = "No Routines Yet",
        ["Routines_NoRoutinesHint"]     = "Create your first routine to start training with structure.",
        ["Routines_NewRoutine"]         = "+ NEW ROUTINE",

        // Settings
        ["Settings_Title"]              = "Settings",
        ["Settings_Preferences"]        = "PREFERENCES",
        ["Settings_WeightUnit"]         = "Weight Unit",
        ["Settings_WeightUnit_Hint"]    = "Choose your preferred unit for logging weights",
        ["Settings_RestTimer"]          = "Default Rest Timer",
        ["Settings_RestTimer_Hint"]     = "Auto-starts after each logged set",
        ["Settings_DataBackup"]         = "DATA & BACKUP",
        ["Settings_ExportData"]         = "Export Data",
        ["Settings_ExportData_Hint"]    = "Save a backup of all your workouts and data",
        ["Settings_Export"]             = "Export",
        ["Settings_ImportData"]         = "Import Data",
        ["Settings_ImportData_Hint"]    = "Restore from a previous backup file",
        ["Settings_Import"]             = "Import",

        // Profile (Edit)
        ["Profile_Title"]               = "My Profile",
        ["Profile_Nickname"]            = "NICKNAME",
        ["Profile_Birthday"]            = "BIRTHDAY",
        ["Profile_Units"]               = "UNITS",
        ["Profile_Height_Cm"]           = "Height (cm)",
        ["Profile_Height_In"]           = "Height (in)",
        ["Profile_Weight_Kg"]           = "Weight (kg)",
        ["Profile_Weight_Lb"]           = "Weight (lb)",
        ["Profile_HeightLabel_Cm"]      = "HEIGHT (CM)",
        ["Profile_HeightLabel_In"]      = "HEIGHT (IN)",
        ["Profile_WeightLabel_Kg"]      = "WEIGHT (KG)",
        ["Profile_WeightLabel_Lb"]      = "WEIGHT (LB)",
        ["Profile_Gender"]              = "GENDER",
        ["Profile_FitnessGoal"]         = "FITNESS GOAL",
        ["Profile_Language"]            = "LANGUAGE",
        ["Profile_DataBackup"]          = "DATA & BACKUP",
        ["Profile_ExportData"]          = "Export Data",
        ["Profile_ExportData_Hint"]     = "Save a backup of all your workouts and data",
        ["Profile_Export"]              = "Export",
        ["Profile_ImportData"]          = "Import Data",
        ["Profile_ImportData_Hint"]     = "Restore from a previous backup file",
        ["Profile_Import"]              = "Import",
        ["Profile_DarkMode"]            = "Dark Mode",
        ["Profile_DarkMode_Hint"]       = "Easier on the eyes in low light",
        ["Profile_SaveChanges"]         = "Save Changes",
        ["Profile_Saved"]               = "Profile saved!",
        ["Profile_InvalidHeightWeight"] = "Please enter valid height and weight.",
    };

    // ── Romanian strings ───────────────────────────────────────────────────
    private static readonly Dictionary<string, string> _ro = new()
    {
        // Welcome
        ["Welcome_Subtitle"]            = "Inteligența ta personală de antrenament",
        ["Welcome_StartTraining"]       = "ÎNCEPE ANTRENAMENTUL",
        ["Welcome_RestoreBackup"]       = "Restaurează din Backup",

        // Profile Setup
        ["ProfileSetup_Title"]          = "Spune-ne despre tine",
        ["ProfileSetup_Subtitle"]       = "Personalizează experiența ta de antrenament",
        ["Field_Nickname"]              = "PORECLĂ",
        ["Field_NicknamePlaceholder"]   = "ex. Alex",
        ["Field_Birthday"]              = "DATA NAȘTERII",
        ["Field_Units"]                 = "UNITĂȚI",
        ["Field_Gender"]                = "GEN",
        ["Field_Language"]              = "LIMBĂ",
        ["ProfileSetup_Continue"]       = "Continuă",

        // Goal Setup
        ["GoalSetup_Title"]             = "Care este obiectivul tău?",
        ["GoalSetup_Subtitle"]          = "Îți vom personaliza antrenamentele și sugestiile",
        ["Goal_Strength"]               = "Forță",
        ["Goal_Strength_Desc"]          = "Construiește putere și ridică mai greu",
        ["Goal_Hypertrophy"]            = "Hipertrofie",
        ["Goal_Hypertrophy_Desc"]       = "Maximizează masa și definiția musculară",
        ["Goal_FatLoss"]                = "Slăbire",
        ["Goal_FatLoss_Desc"]           = "Arde grăsimea păstrând masa musculară",
        ["Goal_Balanced"]               = "Echilibrat",
        ["Goal_Balanced_Desc"]          = "Fitness și sănătate echilibrate",
        ["GoalSetup_LetsGo"]            = "Să începem!",

        // Onboarding Complete
        ["Complete_Title"]              = "Ești gata!",
        ["Complete_Subtitle"]           = "Profilul tău este gata. E timpul să îți urmărești progresul și să îți îndeplinești obiectivele.",
        ["Complete_Button"]             = "Începe Antrenamentul →",

        // Dashboard
        ["Dashboard_TodaySession"]      = "SESIUNEA DE AZI",
        ["Dashboard_StartSession"]      = "ÎNCEPE SESIUNEA",
        ["Dashboard_Sessions"]          = "SESIUNI",
        ["Dashboard_ThisWeek"]          = "ACEASTĂ SĂPTĂMÂNĂ",
        ["Dashboard_Streak"]            = "SERIE",
        ["Dashboard_MyRoutines"]        = "RUTINELE MELE",
        ["Dashboard_SeeAll"]            = "Vezi toate ›",
        ["Dashboard_New"]               = "+ NOU",
        ["Dashboard_NoRoutines"]        = "Nicio rutină încă",
        ["Dashboard_NoRoutinesHint"]    = "Apasă '+ NOU' pentru a crea prima ta rutină.",
        ["Dashboard_GoodMorning"]       = "BUNĂ DIMINEAȚA",
        ["Dashboard_GoodAfternoon"]     = "BUNĂ ZIUA",
        ["Dashboard_GoodEvening"]       = "BUNĂ SEARA",
        ["Dashboard_FreeWorkout"]       = "Antrenament Liber",
        ["Dashboard_FreeWorkoutHint"]   = "Adaugă exerciții din mers",

        // History
        ["History_Title"]               = "Istoric",
        ["History_SessionsThisMonth"]   = "Sesiuni luna aceasta",
        ["History_TotalVolume"]         = "Volum total",
        ["History_WeeklyVolumeKg"]      = "VOLUM SĂPTĂMÂNAL (KG)",
        ["History_4WeekTrend"]          = "TENDINȚĂ 4 SĂPTĂMÂNI",
        ["History_RecentSessions"]      = "SESIUNI RECENTE",
        ["History_NoWorkouts"]          = "Niciun antrenament găsit",
        ["History_NoWorkoutsHint"]      = "Completează un antrenament pentru a-ți vedea istoricul",

        // Analytics
        ["Analytics_Title"]             = "Progres",
        ["Analytics_1RMTrend"]          = "TENDINȚĂ 1RM",
        ["Analytics_NoData"]            = "Nu există date pentru exercițiul / perioada selectată.",
        ["Analytics_MuscleBalance"]     = "ECHILIBRU MUSCULAR",
        ["Analytics_Recovery"]          = "RECUPERARE",
        ["Analytics_Intensity"]         = "INTENSITATE",
        ["Analytics_Insights"]          = "PERSPECTIVE",
        ["Analytics_NoInsights"]        = "Nicio perspectivă încă — continuă să te antrenezi!",

        // Goals
        ["Goals_Title"]                 = "Obiective",
        ["Goals_Subtitle"]              = "Urmărește-ți forța, constanța și echilibrul",
        ["Goals_NoGoals"]               = "Niciun Obiectiv Încă",
        ["Goals_NoGoalsHint"]           = "Apasă + pentru a seta primul obiectiv și a urmări progresul.",
        ["Goals_Active"]                = "ACTIVE",
        ["Goals_Completed"]             = "COMPLETATE",
        ["Goals_Tab"]                   = "Obiective",
        ["Goals_PRs"]                   = "Recorduri",
        ["Goals_PageTitle"]             = "Obiective & Recorduri",

        // Suggestions
        ["Suggestions_Title"]           = "Sugestii Inteligente",
        ["Suggestions_Subtitle"]        = "Recomandări personalizate bazate pe datele tale de antrenament",
        ["Suggestions_Empty"]           = "Nicio Sugestie Momentan",
        ["Suggestions_EmptyHint"]       = "Continuă să te antrenezi! Sugestiile vor apărea odată ce ai suficiente date de analizat.",

        // Exercise Library
        ["Exercises_Title"]             = "Biblioteca de Exerciții",
        ["Exercises_Subtitle"]          = "Răsfoiește și gestionează exerciții",
        ["Exercises_SearchPlaceholder"] = "Caută exerciții…",
        ["Exercises_NoExercises"]       = "Niciun exercițiu găsit",
        ["Exercises_NoExercisesHint"]   = "Încearcă să ajustezi căutarea sau filtrele",
        ["Exercises_Custom"]            = "Personalizat",

        // Workout / Routines
        ["Routines_YourRoutines"]       = "RUTINELE TALE",
        ["Routines_Train"]              = "Antrenează-te",
        ["Routines_NoRoutines"]         = "Nicio Rutină Încă",
        ["Routines_NoRoutinesHint"]     = "Creează prima ta rutină pentru a te antrena cu structură.",
        ["Routines_NewRoutine"]         = "+ RUTINĂ NOUĂ",

        // Settings
        ["Settings_Title"]              = "Setări",
        ["Settings_Preferences"]        = "PREFERINȚE",
        ["Settings_WeightUnit"]         = "Unitate de Greutate",
        ["Settings_WeightUnit_Hint"]    = "Alege unitatea preferată pentru înregistrarea greutăților",
        ["Settings_RestTimer"]          = "Temporizator de Odihnă Implicit",
        ["Settings_RestTimer_Hint"]     = "Pornește automat după fiecare serie înregistrată",
        ["Settings_DataBackup"]         = "DATE & BACKUP",
        ["Settings_ExportData"]         = "Exportă Date",
        ["Settings_ExportData_Hint"]    = "Salvează un backup al tuturor antrenamentelor și datelor",
        ["Settings_Export"]             = "Exportă",
        ["Settings_ImportData"]         = "Importă Date",
        ["Settings_ImportData_Hint"]    = "Restaurează dintr-un fișier de backup anterior",
        ["Settings_Import"]             = "Importă",

        // Profile (Edit)
        ["Profile_Title"]               = "Profilul Meu",
        ["Profile_Nickname"]            = "PORECLĂ",
        ["Profile_Birthday"]            = "DATA NAȘTERII",
        ["Profile_Units"]               = "UNITĂȚI",
        ["Profile_Height_Cm"]           = "Înălțime (cm)",
        ["Profile_Height_In"]           = "Înălțime (inch)",
        ["Profile_Weight_Kg"]           = "Greutate (kg)",
        ["Profile_Weight_Lb"]           = "Greutate (lb)",
        ["Profile_HeightLabel_Cm"]      = "ÎNĂLȚIME (CM)",
        ["Profile_HeightLabel_In"]      = "ÎNĂLȚIME (INCH)",
        ["Profile_WeightLabel_Kg"]      = "GREUTATE (KG)",
        ["Profile_WeightLabel_Lb"]      = "GREUTATE (LB)",
        ["Profile_Gender"]              = "GEN",
        ["Profile_FitnessGoal"]         = "OBIECTIV FITNESS",
        ["Profile_Language"]            = "LIMBĂ",
        ["Profile_DataBackup"]          = "DATE & BACKUP",
        ["Profile_ExportData"]          = "Exportă Date",
        ["Profile_ExportData_Hint"]     = "Salvează un backup al tuturor antrenamentelor și datelor",
        ["Profile_Export"]              = "Exportă",
        ["Profile_ImportData"]          = "Importă Date",
        ["Profile_ImportData_Hint"]     = "Restaurează dintr-un fișier de backup anterior",
        ["Profile_Import"]              = "Importă",
        ["Profile_DarkMode"]            = "Mod Întunecat",
        ["Profile_DarkMode_Hint"]       = "Mai ușor pentru ochi la lumină slabă",
        ["Profile_SaveChanges"]         = "Salvează Modificările",
        ["Profile_Saved"]               = "Profil salvat!",
        ["Profile_InvalidHeightWeight"] = "Introduceți înălțimea și greutatea valide.",
    };
}
