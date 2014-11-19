case "$1" in
    "S")
        python junk.py 5 2 10 10 5 | tee -a streaming_output
        ;;
    "M")
        python junk.py 25 10 250 250 5 | tee -a streaming_output
        ;;     
    "L")
        python junk.py 100 20 1000 1000 5 | tee -a streaming_output
        ;;
    "Test")
        python junk.py 2 2 2 2 5 | tee -a streaming_output
        ;;
    *)
        echo "This is not a valid input"
        exit 1
 
esac

python Master.py $1 | tee -a streaming_output
a=`ls Results/$1/ -Art | tail -n 1`
mv streaming_output Results/$1/$a/streaming_output
