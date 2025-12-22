// Chat Member Management - Supplemental JavaScript
// This file extends chat.js with member management functionality

// Add event listeners for member management
document.addEventListener('DOMContentLoaded', function () {
    setupMemberManagementListeners();
});

function setupMemberManagementListeners() {
    // Manage Members Button
    const manageMembersBtn = document.getElementById('manage-members-btn');
    if (manageMembersBtn) {
        manageMembersBtn.addEventListener('click', openMembersModal);
    }

    // Add Member Button
    const addMemberBtn = document.getElementById('add-member-btn');
    if (addMemberBtn) {
        addMemberBtn.addEventListener('click', addMemberToRoom);
    }
}

// Open Manage Members Modal
function openMembersModal() {
    const modal = new bootstrap.Modal(document.getElementById('manageMembersModal'));
    modal.show();
    
    // Load room members
    loadRoomMembers();
    
    // Populate user select dropdown
    populateUserSelect();
}

// Load current room members
function loadRoomMembers() {
    // Access state from chat.js
    const currentRoomId = window.chatState?.currentRoomId;
    const connection = window.chatConnection;
    
    if (!currentRoomId || !connection) {
        console.error('No room selected or connection not available');
        return;
    }

    connection.invoke('GetRoomMembers', currentRoomId)
        .catch(err => {
            console.error('Error loading room members:', err);
            displayMembersError('Failed to load members');
        });
}

// Display room members in the modal
function displayRoomMembers(memberIds) {
    const membersList = document.getElementById('current-members-list');
    membersList.innerHTML = '';

    if (!memberIds || memberIds.length === 0) {
        membersList.innerHTML = '<div class="text-center text-muted py-3">No members in this room</div>';
        return;
    }

    // Access allUsers from chat.js state
    const allUsers = window.chatState?.allUsers || [];

    memberIds.forEach(memberId => {
        const user = allUsers.find(u => u.id === memberId);
        const userName = user ? (user.name || user.email) : memberId;

        const memberItem = document.createElement('div');
        memberItem.className = 'list-group-item d-flex justify-content-between align-items-center';
        
        memberItem.innerHTML = `
            <div>
                <i class="bi bi-person-circle me-2"></i>
                <span>${userName}</span>
            </div>
            ${memberId !== currentUserId ? `
                <button class="btn btn-sm btn-outline-danger" onclick="removeMember('${memberId}', '${userName}')">
                    <i class="bi bi-person-dash"></i> Remove
                </button>
            ` : '<span class="badge bg-success">You</span>'}
        `;

        membersList.appendChild(memberItem);
    });
}

// Populate user select dropdown
function populateUserSelect() {
    const userSelect = document.getElementById('user-select');
    userSelect.innerHTML = '<option value="">Select a user...</option>';

    // Access allUsers from chat.js state
    const allUsers = window.chatState?.allUsers || [];
    
    allUsers.forEach(user => {
        if (user. id !== currentUserId) {
            const option = document.createElement('option');
            option.value = user.id;
            option.textContent = user.name || user.email;
            userSelect.appendChild(option);
        }
    });
}

// Add member to room
function addMemberToRoom() {
    const userSelect = document.getElementById('user-select');
    const selectedUserId = userSelect.value;

    if (!selectedUserId) {
        alert('Please select a user');
        return;
    }

    const currentRoomId = window.chatState?.currentRoomId;
    const connection = window.chatConnection;

    if (!currentRoomId || !connection) {
        console.error('No room selected or connection not available');
        return;
    }

    connection.invoke('AddUserToRoom', currentRoomId, selectedUserId)
        .then(() => {
            console.log(`Added user ${selectedUserId} to room`);
            userSelect.value = '';
            // The RoomMembers event will trigger a reload
        })
        .catch(err => {
            console.error('Error adding member:', err);
            alert('Failed to add member to room');
        });
}

// Remove member from room  
function removeMember(userId, userName) {
    const confirmRemove = confirm(`Are you sure you want to remove ${userName} from this room?`);
    if (!confirmRemove) return;

    const currentRoomId = window.chatState?.currentRoomId;
    const connection = window.chatConnection;

    if (!currentRoomId || !connection) {
        console.error('No room selected or connection not available');
        return;
    }

    connection.invoke('RemoveUserFromRoom', currentRoomId, userId)
        .then(() => {
            console.log(`Removed user ${userId} from room`);
            // The RoomMembers event will trigger a reload
        })
        .catch(err => {
            console.error('Error removing member:', err);
            alert('Failed to remove member from room');
        });
}

// Display error in members list
function displayMembersError(message) {
    const membersList = document.getElementById('current-members-list');
    membersList.innerHTML = `<div class="alert alert-danger">${message}</div>`;
}

// Make functions available globally
window.removeMember = removeMember;
