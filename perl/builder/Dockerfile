FROM fission/builder as PKG
FROM perl:slim 

RUN apt-get update ;  apt-get install -y gcc
RUN cpanm -n Twiggy Getopt::Args Dancer2 LWP::Simple LWP::Protocol::https lib::relative  && rm -rf /root/.cpan*

COPY   --from=PKG /builder /builder

EXPOSE 8001
