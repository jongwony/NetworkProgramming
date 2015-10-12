#include<stdio.h>
#include<unistd.h>

void SendingAnime(int incnum){
	int i;
	printf("Loading...");
	for(i=0;i<incnum;i++){
		printf("#");
	}
	usleep(50000);
	printf("\n\33[F\33[J");
}
