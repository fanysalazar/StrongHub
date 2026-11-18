using StrongHub.Controllers;

namespace StrongHub.Views;

/// <summary>All the controllers the screens need, created once in Program.</summary>
public record AppControllers(
    MainMenuController Menu,
    TrainerController Trainers,
    ClassCatalogController Catalog,
    RoutineController Routines,
    RoutineChangesController RoutineChanges,
    VisitController Visits,
    BadgeController Badges,
    ShameBadgeController ShameBadges,
    TimelineController Timeline,
    RoutineHistoryController RoutineHistory,
    MeasurementController Measurements);
