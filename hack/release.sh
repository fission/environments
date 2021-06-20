path=$1
filter=$2
            cd $path
            if [[ $path == "nodejs" ]]; then path="node"; fi
            if [[ $path == "php7" ]]; then path="php"; fi          
            if [[ "$filter" == *"builder"* ]]; then
                cd builder
                if [[ "$filter" == *"@"* ]]; then
                  ver=$(echo "$filter"| cut -d '@' -f2 );
                  version=$(cat version-$ver);
                  make $path-builder-$ver-img TAG="$version";
                else
                   version=$(cat version); 
                   make $path-builder-img TAG="$version";                                         
                fi

            else
              if [[ "$filter" == *"@"* ]]; then
                  ver=$(echo "$filter"| cut -d '@' -f2 );
                  version=$(cat version-$ver);
                  make $path-env-$ver-img TAG="$version"
                else
                   version=$(cat version);
                  make $path-env-img TAG="$version" ;                   
                fi                       
            fi
            