# Bulk Team Assignment Feature

## Overview
The Bulk Team Assignment feature provides a fast, efficient way to assign multiple teams to divisions and set their seed numbers in the tournament. This interface allows administrators to quickly configure team assignments with automatic saving functionality.

## Location
**Admin Area**: `/Admin/TournamentTeams/BulkAssign`

**Access**: From the Tournament Teams Index page, click the "Bulk Assign Teams" button (green button with list-check icon) next to the "Add Team" button.

## Features

### 1. Team List Display
- Shows all teams in the system (both assigned and unassigned)
- Displays team name, school, current division, seed number, and assignment status
- Color-coded status badges:
  - **Green**: Team is assigned to a division
  - **Gray**: Team is not assigned to the tournament

### 2. Division Selection
- Radio button groups for each team
- Options include:
  - Available divisions (e.g., BOYS, GIRLS)
  - "None" option to remove team from tournament
- Visual feedback with highlighted buttons using green/gold theme colors

### 3. Seed Number Input
- Numeric input field (0-100 range)
- Automatically disabled when no division is selected
- Enabled when a division is assigned

### 4. Auto-Save Functionality
- Changes are saved automatically via JavaScript
- **Division changes**: Save immediately upon selection
- **Seed number changes**: Debounced save (500ms delay) to prevent excessive API calls
- Visual feedback:
  - Spinning indicator during save operation
  - Green row highlight on successful save
  - Red row highlight on error

### 5. Status Indicators
- Real-time status updates after each save
- Loading spinner during save operations
- Success/error animations for user feedback

## Technical Implementation

### Files Created

#### 1. ViewModel (`BulkTeamAssignmentViewModel.cs`)
```csharp
- BulkTeamAssignmentViewModel: Main view model
- TeamAssignmentItem: Individual team data
- BulkTeamAssignmentUpdateModel: Update request model
```

#### 2. Controller Actions (`TournamentTeamsController.cs`)
```csharp
- BulkAssign (GET): Loads the bulk assignment view
- BulkAssignUpdate (POST): Handles individual team updates via AJAX
```

#### 3. View (`BulkAssign.cshtml`)
- Responsive table layout
- Radio button groups for division selection
- Numeric inputs for seed numbers
- Status indicators and loading spinners

#### 4. JavaScript (`bulk-assign.js`)
- Event handlers for radio buttons and inputs
- Debounce function for seed number changes
- AJAX POST requests to save changes
- Visual feedback animations

#### 5. CSS (`bulk-assign.css`)
- Table transition animations
- Green/gold theme colors
- Responsive design adjustments
- Loading spinner animations

### API Endpoint

**URL**: `/Admin/TournamentTeams/BulkAssignUpdate`

**Method**: POST

**Request Body**:
```json
{
  "teamId": "guid",
  "divisionId": "guid or null",
  "seedNumber": 0-100
}
```

**Response**:
```json
{
  "success": true/false,
  "message": "status message"
}
```

## Usage Instructions

### Assigning Teams to Divisions

1. Navigate to **Admin > Tournament Teams**
2. Click the **"Bulk Assign Teams"** button
3. For each team:
   - Select the appropriate division by clicking the radio button
   - Enter the seed number (if needed)
4. Changes are saved automatically
5. Watch for the status indicator to confirm the save

### Removing Teams from Tournament

1. Navigate to the Bulk Assign page
2. Find the team you want to remove
3. Click the **"None"** radio button
4. The team will be automatically removed from the tournament

### Changing Team Assignments

1. Simply click a different division radio button
2. Update the seed number if needed
3. Changes are saved automatically

## Best Practices

1. **Review Before Assigning**: Ensure you have the correct division structure set up before bulk assigning teams
2. **Seed Numbers**: Use consistent seed numbering (e.g., 1-16 for a 16-team division)
3. **Network Connection**: Ensure stable internet connection as saves happen automatically
4. **Visual Confirmation**: Always wait for the status indicator to confirm successful save

## Troubleshooting

### Changes Not Saving
- Check browser console for JavaScript errors
- Verify network connection
- Ensure active tournament exists
- Refresh the page and try again

### Seed Number Disabled
- Seed number input is disabled when no division is selected
- Select a division first, then the seed number field will become enabled

### Status Not Updating
- Wait for the save operation to complete (watch for spinner)
- Check that the division exists in the tournament
- Verify team exists in the system

## Performance Considerations

- **Debouncing**: Seed number changes are debounced by 500ms to prevent excessive API calls
- **Individual Saves**: Each team is saved individually for reliability
- **Real-time Feedback**: Users get immediate visual feedback for each operation

## Integration Points

### Services Used
- `IActiveTournamentService`: Get active tournament and available teams/divisions
- `IActiveTournamentService.AddTeamAsync()`: Add new team assignments
- `IActiveTournamentService.SetTeamAsync()`: Update existing assignments
- `IActiveTournamentService.RemoveTeamAsync()`: Remove teams from tournament

### Related Features
- Tournament Teams Index
- Individual Team Assignment (Create/Edit)
- Tournament Management

## Future Enhancements

Potential improvements for future versions:
- Bulk import from CSV/Excel
- Copy division assignments from previous tournament
- Validation warnings for duplicate seed numbers
- Undo/redo functionality
- Filter teams by school or current status
- Sort teams by various criteria

## Logging

The feature includes comprehensive logging:
- Info level: Successful team assignments/updates/removals
- Warning level: Active tournament not found
- Error level: Failed operations with exception details

All logs include relevant context (TeamId, DivisionId, SeedNumber) for troubleshooting.
