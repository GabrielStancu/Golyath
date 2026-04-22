$selected = @(
  "Barbell_Bench_Press_-_Medium_Grip","Dumbbell_Bench_Press","Machine_Bench_Press","Wide-Grip_Barbell_Bench_Press","Close-Grip_Barbell_Bench_Press",
  "Barbell_Incline_Bench_Press_-_Medium_Grip","Incline_Dumbbell_Press","Leverage_Incline_Chest_Press","Decline_Barbell_Bench_Press","Decline_Dumbbell_Bench_Press",
  "Dumbbell_Flyes","Incline_Dumbbell_Flyes","Cable_Crossover","Cable_Chest_Press","Pushups","Dips_-_Chest_Version",
  "Pullups","Chin-Up","Wide-Grip_Lat_Pulldown","Close-Grip_Front_Lat_Pulldown","V-Bar_Pulldown","Underhand_Cable_Pulldowns",
  "Bent_Over_Barbell_Row","Seated_Cable_Rows","One-Arm_Dumbbell_Row","T-Bar_Row_with_Handle","Inverted_Row","Elevated_Cable_Rows",
  "Barbell_Deadlift","Romanian_Deadlift","Hyperextensions_Back_Extensions","Good_Morning",
  "Barbell_Shoulder_Press","Seated_Dumbbell_Press","Machine_Shoulder_Military_Press","Arnold_Dumbbell_Press","Front_Dumbbell_Raise","Front_Cable_Raise",
  "Side_Lateral_Raise","Cable_Seated_Lateral_Raise","Seated_Side_Lateral_Raise","Upright_Barbell_Row","Face_Pull",
  "Bent_Over_Dumbbell_Rear_Delt_Raise_With_Head_On_Bench","Cable_Rear_Delt_Fly","Reverse_Machine_Flyes","Seated_Bent-Over_Rear_Delt_Raise",
  "Barbell_Curl","Dumbbell_Bicep_Curl","Hammer_Curls","Incline_Dumbbell_Curl","Concentration_Curls","Preacher_Curl","EZ-Bar_Curl","Cable_Preacher_Curl","High_Cable_Curls",
  "Triceps_Pushdown","Triceps_Pushdown_-_Rope_Attachment","EZ-Bar_Skullcrusher","Dips_-_Triceps_Version","Overhead_Triceps","Cable_Rope_Overhead_Triceps_Extension","Lying_Dumbbell_Tricep_Extension","Machine_Triceps_Extension","Tricep_Dumbbell_Kickback",
  "Barbell_Squat","Front_Barbell_Squat","Leg_Press","Hack_Squat","Leg_Extensions","Dumbbell_Squat","Goblet_Squat","Barbell_Lunge",
  "Lying_Leg_Curls","Seated_Leg_Curl","Stiff-Legged_Barbell_Deadlift","Dumbbell_Lunges","Barbell_Walking_Lunge",
  "Barbell_Hip_Thrust","Barbell_Glute_Bridge","Glute_Kickback","Sumo_Deadlift",
  "Standing_Calf_Raises","Seated_Calf_Raise","Calf_Press_On_The_Leg_Press_Machine",
  "Crunches","Cable_Crunch","Hanging_Leg_Raise","Plank","Russian_Twist","Decline_Crunch","Leg_Pull-In","Ab_Roller","Side_Bridge","Flutter_Kicks",
  "Dumbbell_Step_Ups","Seated_Barbell_Military_Press","Bench_Dips","Weighted_Pull_Ups","Trap_Bar_Deadlift"
)

$id = 1
foreach ($ex in $selected) {
  $j = Get-Content "d:\Projects\Golyath\exercises\$ex.json" -Raw | ConvertFrom-Json
  $pri = if ($j.primaryMuscles) { ($j.primaryMuscles | Select-Object -First 1) } else { '' }
  $sec = if ($j.secondaryMuscles) { $j.secondaryMuscles -join ',' } else { '' }
  $mech = $j.mechanic
  $equip = $j.equipment
  "$id|$ex|$($j.name)|$equip|$pri|$sec|$mech" | Write-Host
  $id++
}
