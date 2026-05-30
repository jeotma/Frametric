$temp = 'c:\Users\Jeotm\Documents\PersonalProjects\Frametric\SampleData\temp_skye'
Expand-Archive -Path 'c:\Users\Jeotm\Documents\PersonalProjects\Frametric\SampleData\letterboxd-skye7-2026-05-29-23-07-utc.zip' -DestinationPath $temp -Force

Write-Output "Diary:"
$diary = Get-Content "$temp\diary.csv"
$diary.Count

Write-Output "Ratings:"
$ratings = Get-Content "$temp\ratings.csv"
$ratings.Count

Write-Output "Watchlist:"
$watchlist = Get-Content "$temp\watchlist.csv"
$watchlist.Count

Write-Output "Likes:"
$likes = Get-Content "$temp\likes\films.csv"
$likes.Count

Write-Output "Total Rows (including headers):"
$diary.Count + $ratings.Count + $watchlist.Count + $likes.Count

# To get an idea of unique movie titles
$allMovies = @()
$allMovies += $diary | Select-Object -Skip 1 | ForEach-Object { ($_ -split ',')[1] }
$allMovies += $ratings | Select-Object -Skip 1 | ForEach-Object { ($_ -split ',')[1] }
$allMovies += $watchlist | Select-Object -Skip 1 | ForEach-Object { ($_ -split ',')[1] }
$allMovies += $likes | Select-Object -Skip 1 | ForEach-Object { ($_ -split ',')[1] }

Write-Output "Unique Movies (approximate, based on title):"
($allMovies | Sort-Object -Unique).Count

Remove-Item -Recurse -Force $temp
