FROM perl:slim as builder

RUN apt-get update ;  apt-get install -y gcc
RUN cpanm -n Twiggy Getopt::Args Dancer2 JSON lib::relative

FROM   perl:slim
COPY    --from=builder /usr/local /usr/local
COPY    server.pl /server.pl
WORKDIR /

EXPOSE 8888
ENTRYPOINT ["/server.pl"]
