#!/usr/bin/awk -f

{
    # Match the line containing "static string BuildNo"
    if ($0 ~ /static string BuildNo/) {
        # Use a regex to extract the number in quotes
        match($0, /"([0-9]+)"/, build)
        if (build[1]) {
            # Increment the number
            buildNo = build[1] + 1
            # Pad the number to retain the same format (e.g., 0046)
            newBuildNo = sprintf("%04d", buildNo)
            # Replace the old number with the new one
            sub(/"[0-9]+"/, "\"" newBuildNo "\"")
        }
    }
    # Print the (possibly modified) line
    print
}
