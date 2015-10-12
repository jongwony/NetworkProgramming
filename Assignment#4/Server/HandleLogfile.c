#include<stdio.h>

FILE *stream;

void HandleLogfile(char *echoBuffer){
    if((stream = fopen("echo_history.log","a+")) == NULL){  // append continuously(remove EOF)
        printf("'echo_history.log' is not opened\n");
    }
    fprintf(stream, "%s\n", echoBuffer);                    // append log file
    fclose(stream);
}
