/**
 * Generate Next Round - Form Interactivity
 * Handles dynamic form behavior for the Generate Next Round page
 */

(function () {
    'use strict';

    // Initialize when DOM is ready
    $(document).ready(function () {
        initializeRoundSelector();
        initializeGroupConfiguration();
        initializeImmediateActions();
    });

    /**
     * Initialize the round selector dropdown
     * Disables the current round option to prevent selecting the same round
     */
    function initializeRoundSelector() {
        const currentRoundId = $('input[name="CurrentRoundId"]').val();
        
        if (currentRoundId) {
            // Disable the current round option in the dropdown
            $('#roundSelector option[value="' + currentRoundId + '"]').prop('disabled', true);
        }
    }

    /**
     * Initialize group configuration type selector
     * Updates the label dynamically based on the selected configuration type
     */
    function initializeGroupConfiguration() {
        const $groupConfigType = $('#groupConfigType');
        const $configLabel = $('#configLabel');

        // Function to update the label
        function updateConfigLabel() {
            const configType = $groupConfigType.val();
            if (configType === 'TeamsPerGroup') {
                $configLabel.text('Teams per group');
            } else {
                $configLabel.text('Groups in round');
            }
        }

        // Update label on change
        $groupConfigType.on('change', updateConfigLabel);

        // Initialize on page load
        updateConfigLabel();
    }

    /**
     * Initialize immediate actions checkboxes
     * Ensures logical dependency between "Assign Teams" and "Generate Matches"
     */
    function initializeImmediateActions() {
        const $assignTeams = $('#assignTeams');
        const $generateMatches = $('#generateMatches');

        // When "Generate Matches" is checked, automatically check "Assign Teams"
        // because you can't generate matches without teams
        $generateMatches.on('change', function () {
            if ($(this).is(':checked')) {
                $assignTeams.prop('checked', true);
            }
        });

        // When "Assign Teams" is unchecked, automatically uncheck "Generate Matches"
        // because you can't generate matches if teams aren't assigned
        $assignTeams.on('change', function () {
            if (!$(this).is(':checked')) {
                $generateMatches.prop('checked', false);
            }
        });
    }

})();
